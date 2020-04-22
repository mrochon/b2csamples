﻿# once only: Install-Module -Name MSAL.PS, AzureAD-preview

$conf = ".\setupSettings.json"
$settings = Get-Content -Path $conf -ErrorAction Stop | Out-String | ConvertFrom-Json

$iefConfPath = ".\conf.json"

if (Test-Path -Path $iefConfPath) {
    $iefConf = Get-Content -Path $iefConfPath -ErrorAction Continue | Out-String | ConvertFrom-Json
} else {
    $iefConf = [PSCustomObject]@{
        Prefix = "MT"
        SUSI_UI = "https://b2cdatastore.blob.core.windows.net/uix/IdPSelect.html"
        SignInOnly_UI = "https://b2cdatastore.blob.core.windows.net/uix/SignInOnly.html"
    }
}

################### Are the web app urls available?  #####################
try {
    $page = invoke-webrequest ("https://{0}.azurewebsites.net" -f $settings.webApp.name)
    "{0} already exists. Please try a different name" -f $settings.webApp.name
    return
} catch {
}
try {
    $page = invoke-webrequest ("https://{0}.azurewebsites.net" -f $settings.webAPI.name)
    "{0} already exists. Please try a different name" -f $settings.webAPI.name
    return
} catch {
}

################### Login to AAD and Az ##################################
Write-Host ("Login to Azure with an account with sufficient privilege to create a resource group and web apps")
Connect-AzAccount -ErrorAction Stop
Write-Host ("Login to your B2C directory with an account with sufficient privileges to register applications")
Connect-AzureAD -TenantId $settings.b2cTenant -ErrorAction Stop
$b2c = Get-AzureADCurrentSessionInfo -ErrorAction stop
$azure = Get-AzContext -ErrorAction stop


##################  Get AAD B2C extension app ############################
if ([string]::IsNullOrEmpty($iefConf.ExtAppId)) {
    $extensionApp = get-azureadapplication -filter "displayName eq 'b2c-extensions-app. Do not modify. Used by AADB2C for storing user data.'"
    $iefConf.ExtAppId = $extensionApp.AppId
    $iefConf.ExtObjectId = $extensionApp.ObjectId
    out-file -FilePath $iefConfPath -inputobject (ConvertTo-Json $iefConf)
}

###############################Get Graph REST token ##############################################
$token = Get-MsalToken -clientId $settings.script.clientId -redirectUri $settings.script.redirectUri -Tenant $settings.b2cTenant -Scopes $settings.script.scopes

$headers = @{
   'Content-Type' = 'application/json';
   'Authorization' = ("Bearer {0}" -f $token.AccessToken);
}

############### Register apps in B2C ###########################
$url = "https://graph.microsoft.com/beta/applications"
$webAppReg = Invoke-RestMethod -Uri ("{0}?`$filter=displayName eq '{1}'" -f $url, $settings.webApp.name) -Method Get -Headers $headers 
if ($webAppReg.value.Count -eq 0) {
    $body = @{
        "displayName" = $settings.webApp.name;
        "isFallbackPublicClient" = $false;
        "web" = @{
            "redirectUris" = foreach($p in @("mtsusi-firsttenant", "mtsusi2", "redeem", "mtsusint")){ "https://{0}.azurewebsites.net/signin-{1}" -f $settings.webApp.name, $p };
            "implicitGrantSettings" = @{
              "enableAccessTokenIssuance" = $false;
              "enableIdTokenIssuance" = $false
            }
        };
        "identifierUris" = @(("https://{0}/{1}" -f $settings.b2cTenant, $settings.webApp.name));
    }
    $webAppReg = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body (ConvertTo-Json -Depth 3 $body)
}
$webAPIReg = Invoke-RestMethod -Uri ("{0}?`$filter=displayName eq '{1}'" -f $url, $settings.webAPI.name) -Method Get -Headers $headers 
if ($webAPIReg.value.Count -eq 0) {
    $body = @{
        "displayName" = $settings.webAPI.name;
        "isFallbackPublicClient" = $false;
        "web" = @{
            "redirectUris" =  @(("https://{0}.com" -f $settings.webAPI.name))
            "implicitGrantSettings" = @{
              "enableAccessTokenIssuance" = $false;
              "enableIdTokenIssuance" = $false
            }
        };
        "identifierUris" = @(("https://{0}/b2crestapi" -f $settings.b2cTenant))
    }
    $webAPIReg = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body (ConvertTo-Json -Depth 3 $body)
    #################### Add scopes #########################
    $body = @{
        "api" = @{
            "requestedAccessTokenVersion" = 2;
            "oauth2PermissionScopes" = @(
                @{
                    "adminConsentDescription" = "Allow reading all members";
                    "adminConsentDisplayName" = "Read members";
                    "id" = (New-Guid);
                    "isEnabled" = $true;
                    "type" = "User";
                    "userConsentDescription" = "Allow reading all members";
                    "userConsentDisplayName" = "Read members";
                    "value" = "Members.ReadAll"
                });
        }
    }
    $url = "https://graph.microsoft.com/beta/applications/{0}" -f $webAPIReg.id
    $settings.script.scopes = Invoke-RestMethod -Uri $url  -Method Patch -Headers $headers -Body (ConvertTo-Json -Depth 3 $body)
}

################## Register ServicePrincipals SP #########################
$url = "https://graph.microsoft.com/beta/serviceprincipals"
$webAPISP = Invoke-RestMethod -Uri ("{0}?`$filter=appId eq '{1}'" -f $url, $webAPIReg.appId) -Method Get -Headers $headers 
if ($webAPISP.value.Count -eq 0) {
    $body = @{
      "appId" = $webAPIReg.appId;
      "displayName" = $webAPIReg.displayName
    }
    $webAPISP = Invoke-RestMethod -Uri "https://graph.microsoft.com/beta/serviceprincipals"  -Method Post -Headers $headers -Body (ConvertTo-Json $body)
}
$webAppSP = Invoke-RestMethod -Uri ("{0}?`$filter=appId eq '{1}'" -f $url, $webAppReg.appId) -Method Get -Headers $headers 
if ($webAppSP.value.Count -eq 0) {
    $body = @{
      "appId" = $webAppReg.appId;
      "displayName" = $webAppReg.displayName
    }
    $webAppSP = Invoke-RestMethod -Uri $url  -Method Post -Headers $headers -Body (ConvertTo-Json $body)
    $url = "https://graph.microsoft.com/beta/servicePrincipals?$filter=publisherName eq 'Microsoft Graph' or (displayName eq 'Microsoft Graph' or startswith(displayName,'Microsoft Graph'))"
    $graphSP = Invoke-RestMethod -Uri $url -Method Get -Headers $headers
    $today = Get-Date
    $body = @{
      "clientId" = $webAppSP.id;
      "consentType" = "AllPrincipals";
      "expiryTime" = (Get-Date -Year ($today.Year + 3) -Format o);
      "principalId" = $null;
      "resourceId" = ($graphSP.value[0].id)
      "scope" = "openid offline_access"
    }
    Invoke-RestMethod -Uri "https://graph.microsoft.com/beta/oauth2PermissionGrants"  -Method Post -Headers $headers -Body (ConvertTo-Json $body)
    ##################### Grant permission for web app to call the API #########################
    $body = @{
      "clientId" = $webAppSP.id;
      "consentType" = "AllPrincipals";
      "expiryTime" = (Get-Date -Year ($today.Year + 3) -Format o);
      "principalId" = $null;
      "resourceId" = $webAPISP.id
      "scope" = "Members.ReadAll"
    }
    Invoke-RestMethod -Uri "https://graph.microsoft.com/beta/oauth2PermissionGrants"  -Method Post -Headers $headers -Body (ConvertTo-Json $body)
}

################### Add secret key to web app ###################################
$body = @{
    "passwordCredentials" = @(
    @{
      "customKeyIdentifier" = "ScriptRequest";
      "endDateTime" = "2299-12-31T00:00:00";
    })
}
$url = "https://graph.microsoft.com/beta/applications/{0}/addPassword" -f $webAppReg.id
$webAppCreds = Invoke-RestMethod -Uri $url  -Method Post -Headers $headers -Body (ConvertTo-Json -Depth 3 $body)

################### Create signing key for invitations #################
Add-Type -AssemblyName System.Security
[Reflection.Assembly]::LoadWithPartialName("System.Security")
$rijndael = new-Object System.Security.Cryptography.RijndaelManaged
$rijndael.GenerateKey()
$secret = ([Convert]::ToBase64String($rijndael.Key))
$rijndael.Dispose()
$body = @{
    
}

##########################################################################

$svcPlan = $settings.webApp.name
#$webAppSvc = "{0}$(Get-Random)" -f ($settings.webApp.name)
#$webAPISvc = "{0}$(Get-Random)" -f ($settings.webAPI.name)
$webAppSvc = $settings.webApp.name
$webAPISvc = $settings.webAPI.name
Write-Host ("Deploying apps to {0}" -f $azure.Name)
Write-Host ("Configuring {0} B2C tenant" -f $b2c.TenantDomain)

#########################################################################
Write-Host ("Registering AAD (client credentials) REST API app and service principal")
$access = New-Object -TypeName Microsoft.Open.AzureAD.Model.RequiredResourceAccess
$access.ResourceAppId = "00000003-0000-0000-c000-000000000000"
$groupRW = New-Object -TypeName Microsoft.Open.AzureAD.Model.ResourceAccess
$groupRW.Id = "62a82d76-70ea-41e2-9197-370581804d09"
$groupRW.Type = "Role"
$userRead = New-Object -TypeName Microsoft.Open.AzureAD.Model.ResourceAccess
$userRead.Id = "df021288-bdef-4463-88db-98f22de89214"
$userRead.Type = "Role"
$access.ResourceAccess = @($groupRW, $userRead)
$ccredsApp = New-AzureADApplication `
    -IdentifierUris ("app://{0}" -f $settings.webAPI.name) `
    -DisplayName ("{0}-clientcreds" -f $settings.webAPI.name) `
    -RequiredResourceAccess $access `
    -Oauth2AllowImplicitFlow $false 
$ccredPwd = New-AzureADApplicationPasswordCredential -ObjectId $ccredsApp.objectId -CustomKeyIdentifier "PS Generated"
$ccredsSP = New-AzureADServicePrincipal -AppId $ccredsApp.appId -AccountEnabled $true


##################### Create web app svc for the Web App #############################
Write-Host ("Creating Azure Resource Group, Service Plan and Web Apps (REST and UI)")
New-AzResourceGroup -Name $settings.resourceGroup -location $settings.location
New-AzAppServicePlan -Name $svcPlan -location $settings.location -ResourceGroupName $settings.resourceGroup -Tier B1
# Web APP and Web API
$app = New-AzWebApp -Name $webAppSvc `
    -location $settings.location `
    -AppServicePlan $svcPlan `
    -ResourceGroupName $settings.resourceGroup
$props = @{
    "AzureAD:TenantId" = $b2c.TenantId.ToString();
    "AzureAD:ClientId" = $webAppReg.appId.ToString();
    "AzureAD:ClientSecret" = $webAppCreds.secretText;
    "AzureAD:Domain" = $b2c.TenantDomain;
    RestUrl = "https://{0}.azurewebsites.net" -f $webAPISvc;
    AllowedHosts = "*";
}
Set-AzWebApp -Name $webAppSvc `
    -ResourceGroupName $settings.resourceGroup `
    -AppSettings $props

        #"Invitation:Domain" = $b2c.TenantDomain;
    #"Invitation:ClientId" = $webAppReg.appId.ToString();
    #"Invitation:InvitationPolicy" = "B2C_1A_MTInvitation";
    #"Invitation:Issuer" = "b2cmultitenant";
    #"Invitation:Audience" = "b2cmultitenant";
    #"Invitation:ValidityHours" = "72";
    #"Invitation:RedirectPath" = "members/redeem";
    #"Invitation:SigningKey" = $settings.InvitationKey;

####################### Create X509 cert for authn to REST #######################################
$certPath = ".\{0}.cer" -f $settings.webAPI.name
if (Test-Path -Path $certPath) {
    $cer = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2
    $cer.Import($certPath)
} else {
    Write-Host ("Creating X509 cert for IEF policy authentication to allow REST calls")
    $cert = New-SelfSignedCertificate `
        -KeyExportPolicy Exportable `
        -Subject ("CN={0}.{1}" -f $settings.webApi.name, $b2c.TenantDomain) `
        -KeyAlgorithm RSA `
        -KeyLength 2048 `
        -KeyUsage DigitalSignature `
        -NotAfter (Get-Date).AddMonths(12) `
        -CertStoreLocation "Cert:\CurrentUser\My"
    $pfxPwd = ConvertTo-SecureString -String $settings.X509KeyPassword -Force -AsPlainText
    Export-Certificate -Cert $cert -FilePath $certPath
    $pfxPath = (".\{0}.pfx" -f $settings.webAPI.name)
    Get-ChildItem -Path ("cert:\CurrentUser\My\{0}" -f $cert.Thumbprint) | Export-PfxCertificate -FilePath $pfxPath -Password $pfxPwd
    $pfx = Get-Content -Path $pfxPath
    $pkcs12=[Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($pfx))

    $keysetName = "B2C_1A_RESTClientCert"
    try {
        $key = Invoke-RestMethod -Uri ("https://graph.microsoft.com/beta/trustFramework/keySets/{0}" -f $keysetName) -Method Delete -Headers $headers
    } catch { 
        # ok if does not exist
    }
    $body = @{
        id = $keysetName
    }
    Invoke-RestMethod -Uri "https://graph.microsoft.com/beta/trustFramework/keySets" -Method Post -Headers $headers -Body (ConvertTo-Json $body)
    $url = ("https://graph.microsoft.com/beta/trustFramework/keySets/{0}/uploadPkcs12" -f $keysetName)
    $body = @{
        key = $pkcs12
        password = $settings.X509KeyPassword
    }
    $key = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body (ConvertTo-Json $body)
}

##########################  Create Azure Web Apps services for REST API app ##############################
$api = New-AzWebApp -Name $webAPISvc `
    -location $settings.location `
    -AppServicePlan $svcPlan `
    -ResourceGroupName $settings.resourceGroup  `
    -ErrorAction stop

$props = @{
    "AuthCert:thumbprint" = $cert.Thumbprint;
    "AuthCert:issuer" = $cert.Issuer;
    "AuthCert:subject" = $cert.Subject;
    "B2C:Instance" = "https://{0}" -f $b2c.TenantDomain;
    "B2C:TenantId" = $b2c.TenantId.ToString();
    "B2C:ClientId" = $webAPIReg.appId.ToString();
    "B2C:Policy" = "b2c_1a_mtsusi2";
    "ClientCreds:Instance" = "https://login.microsoftonline.com/";
    "ClientCreds:TenantId" = $b2c.TenantId.ToString();
    "ClientCreds:ClientId" = $ccredsApp.appId;
    "ClientCreds:ClientSecret" = $ccredPwd.Value;
    AllowedHosts = "*";
    WEBSITE_LOAD_CERTIFICATES = $cert.Thumbprint;
    WEBSITE_NODE_DEFAULT_VERSION = "6.9.1";
}
$api = Set-AzWebApp -Name $webAPISvc `
        -ResourceGroupName $settings.resourceGroup `
        -AppSettings $props
$api.ClientCertEnabled = $true
$api.ClientCertExclusionPaths = "/tenant/oauth2"
$api = Set-AzWebApp -WebApp $api

New-AzWebAppSSLBinding -WebAppName $webAPISvc `
        -ResourceGroupName $settings.resourceGroup `
        -Name 'IEFRest' `
        -CertificateFilePath $certPath
pwd
$iefConf | Add-Member -MemberType NoteProperty -Name 'RestTenantCreate' -Value ("https://{0}/tenant" -f $api.DefaultHostName)
$iefConf | Add-Member -MemberType NoteProperty -Name 'RestGetOrJoinTenant' -Value ("https://{0}/tenant/member" -f $api.DefaultHostName)
$iefConf | Add-Member -MemberType NoteProperty -Name 'RestGetTenantForUser' -Value ("https://{0}/tenant/currmember" -f $api.DefaultHostName)
$iefConf | Add-Member -MemberType NoteProperty -Name 'RestGetFirstTenant' -Value ("https://{0}/tenant/first" -f $api.DefaultHostName)
out-file -FilePath $iefConfPath -inputobject (ConvertTo-Json $iefConf)


########################## Create AADCommon for work accounts signin ##################
Write-Host ("Registering AAD Common app to support signin with work address using AD multi-tenant support")
if ([string]::IsNullOrEmpty($iefConf.AADCommonAppId)) {
    $access = New-Object -TypeName Microsoft.Open.AzureAD.Model.RequiredResourceAccess
    $access.ResourceAppId = "00000003-0000-0000-c000-000000000000"
    $openId = New-Object -TypeName Microsoft.Open.AzureAD.Model.ResourceAccess
    $openId.Id = "37f7f235-527c-4136-accd-4a02d197296e"
    $openId.Type = "Scope"
    $offline = New-Object -TypeName Microsoft.Open.AzureAD.Model.ResourceAccess
    $offline.Id = "7427e0e9-2fba-42fe-b0c0-848c9e6a8182"
    $offline.Type = "Scope"
    $access.ResourceAccess = @($openId, $offline)
    $aadCommon = New-AzureADApplication `
        -DisplayName "AADCommon" `
        -RequiredResourceAccess $access `
        -Oauth2AllowImplicitFlow $false `
        -AvailableToOtherTenants $true `
        -ReplyUrls @( ("https://{0}.b2clogin.com/{1}/oauth2/authresp" -f $b2c.TenantDomain.Split('.')[0], $b2c.TenantDomain))

    $aadCommonPwd = New-AzureADApplicationPasswordCredential -ObjectId $aadCommon.objectId -CustomKeyIdentifier "PS Generated"
    $appCommonSP = New-AzureADServicePrincipal -AppId $aadCommon.appId -AccountEnabled $true

    ### Upload secret to IEF
    $keysetName = "B2C_1A_AADCommonSecret"
    $url = ("https://graph.microsoft.com/beta/trustFramework/keySets/{0}" -f $keysetName)
    try {
        Invoke-RestMethod -Uri $url -Method Delete -Headers $headers -ErrorAction Ignore
    } catch {
        $err = $_
    }
    $body = @{
        id = $keysetName
    }
    $keyset = Invoke-RestMethod -Uri "https://graph.microsoft.com/beta/trustFramework/keySets" -Method Post -Headers $headers -Body (ConvertTo-Json $body)
    $url = ("https://graph.microsoft.com/beta/trustFramework/keySets/{0}/uploadSecret" -f $keysetName)
    $body = @{
        use = "sig"
        k = $aadCommonPwd.Value
    }
    $key = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body (ConvertTo-Json $body
    $iefConf | Add-Member -MemberType NoteProperty -Name 'AADCommonAppId' -Value ("https://{0}/tenant" -f $aadCommon.appId)
    $iefConf | Add-Member -MemberType NoteProperty -Name 'AADCommonSecret' -Value ("https://{0}/tenant" -f $keysetName)
    out-file -FilePath $iefConfPath -inputobject (ConvertTo-Json $iefConf)
}

############################### Deploy code ############################################
$props = @{
    token = $settings.webApp.gitToken;
}
Set-AzResource -PropertyObject $props `
-ResourceId /providers/Microsoft.Web/sourcecontrols/GitHub -ApiVersion 2015-08-01 -Force
Start-sleep -s 30

# Configure GitHub deployment from your GitHub repo and deploy once.
# Web App
$props = @{
    repoUrl = $settings.webApp.gitRepo;
    branch = "master";
    build = "kudu";
}
Set-AzResource -PropertyObject $props -ResourceGroupName $settings.resourceGroup `
    -ResourceType Microsoft.Web/sites/sourcecontrols -ResourceName ("{0}/web" -f $webAppSvc) `
    -ApiVersion 2015-08-01 -Force
# WebAPI
$props = @{
    repoUrl = $settings.webAPI.gitRepo;
    branch = "master";
    build = "kudu";
}
Set-AzResource -PropertyObject $props -ResourceGroupName $settings.resourceGroup `
    -ResourceType Microsoft.Web/sites/sourcecontrols -ResourceName ("{0}/web" -f $webAPISvc) `
    -ApiVersion 2015-08-01 -Force

"Please use the Azure portal grant admin consent to permissions needed by the {0}-clientcreds application in your N2C tenant" -f $webAppSvc