# once only: 
# Install-Module -Name MSAL.PS -RequiredVersion 2.5.0.1
# AzureAD-preview
# Install-Module -Name Az -AllowClobber
# 

################### Functions ######################################
function UploadIEFSymKey([string]$keysetName, [string]$value)
{
    "Uploading {0} keyset" -f $keySetName
    $url = ("https://graph.microsoft.com/beta/trustFramework/keySets/B2C_1A_{0}" -f $keysetName)
    try {
        Invoke-RestMethod -Uri $url -Method Delete -Headers $headers -ErrorAction Ignore
    } catch {
        $err = $_
    }
    $body = @{
        id = $keysetName
    }
    $keyset = Invoke-RestMethod -Uri "https://graph.microsoft.com/beta/trustFramework/keySets" -Method Post -Headers $headers -Body (ConvertTo-Json $body)
    $url = ("https://graph.microsoft.com/beta/trustFramework/keySets/B2C_1A_{0}/uploadSecret" -f $keysetName)
    $body = @{
        use = "sig"
        kty = "OCT"
        k = $value
    }
    $key = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body (ConvertTo-Json $body)
}

function Upload-IEFPolicies {
    # get current tenant data
    $b2c = Get-AzureADCurrentSessionInfo -ErrorAction stop   
    $iefRes = Get-AzureADApplication -Filter "DisplayName eq 'IdentityExperienceFramework'"
    $iefProxy = Get-AzureADApplication -Filter "DisplayName eq 'ProxyIdentityExperienceFramework'"

    # load originals
    $policies = @(
        "TrustFrameworkBase.xml",
        "TrustFrameworkExtensions.xml",
        "InvitationExtensions.xml",
        "PasswordReset.xml",
        "MTSUSINT.xml"
        "MTSUSI2.xml",
        "MTSUSI-First.xml",
        "Invitation.xml"
    )
    $wc = New-Object System.Net.WebClient
    foreach($policyName in $policies) {
        try {
            $url = "https://raw.githubusercontent.com/mrochon/b2csamples/master/Policies/MultiTenant/{0}" -f $policyName
            $p = $wc.DownloadString($url)
        } catch {
            "{0} failed to download" -f $policyName
            return
        }
        $msg = "{0}: uploading" -f $policyName
        Write-Host $msg  -ForegroundColor Green 
        $policy = $p.Replace('yourtenant.onmicrosoft.com', $b2c.TenantDomain)
        $policy = $policy.Replace('ProxyIdentityExperienceFrameworkAppId', $iefProxy.AppId)
        $policy = $policy.Replace('IdentityExperienceFrameworkAppId', $iefRes.AppId)
        $policy = $policy.Replace('PolicyId="B2C_1A_', 'PolicyId="B2C_1A_{0}' -f $settings.policyPrefix)
        $policy = $policy.Replace('/B2C_1A_', '/B2C_1A_{0}' -f $settings.policyPrefix)
        $policy = $policy.Replace('<PolicyId>B2C_1A_', '<PolicyId>B2C_1A_{0}' -f $settings.policyPrefix)

        # replace other placeholders, e.g. {MyRest} with http://restfunc.com. Note replacement string must be in {}
        $special = @('IdentityExperienceFrameworkAppId', 'ProxyIdentityExperienceFrameworkAppId', 'PolicyPrefix')
        foreach($memb in Get-Member -InputObject $iefConf -MemberType NoteProperty) {
            if ($memb.MemberType -eq 'NoteProperty') {
                if ($special.Contains($memb.Name)) { continue }
                $repl = "{{{0}}}" -f $memb.Name
                $policy = $policy.Replace($repl, $memb.Definition.Split('=')[1])
            }
        }
        $xml = [xml] $policy
        $Id = $xml.TrustFrameworkPolicy.PolicyId
        Set-AzureADMSTrustFrameworkPolicy -Content ($policy | Out-String) -Id $Id -ErrorAction Stop | Out-Null
    }
}

##########################################################################
###################     Execution starts here ############################
foreach($moduleName in @("AzureADPreview", "Az")) {
    $m = Get-Module -ListAvailable -Name AzureADPreview
    if ($m -eq $null) {
        "Please install-module {0} before running this command" -f $moduleName
        return
    }
}
$settings = Get-Content -Path ".\setupSettings.json" -ErrorAction Stop | Out-String | ConvertFrom-Json
$iefConfPath = ".\conf.json"
$iefConf = Get-Content -Path $iefConfPath -ErrorAction Continue | Out-String | ConvertFrom-Json

"Update conf with current prefix"
$iefConf.Prefix = $settings.policyPrefix
out-file -FilePath $iefConfPath -inputobject (ConvertTo-Json $iefConf)
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
Connect-AzAccount -SubscriptionId $settings.subscription -ErrorAction Stop
Write-Host ("Login to your B2C directory with an account with sufficient privileges to register B2C applications in the B2C tenant")
$azure = Get-AzContext -ErrorAction stop
Connect-AzureAD -TenantId $settings.b2cTenant -AccountId $azure.Account.Id -ErrorAction Stop
$b2c = Get-AzureADCurrentSessionInfo -ErrorAction stop


##################  Get AAD B2C extension app ############################
"Get AAD B2C extensions app"
$extensionApp = get-azureadapplication -filter "displayName eq 'b2c-extensions-app. Do not modify. Used by AADB2C for storing user data.'"
$iefConf.ExtAppId = $extensionApp.AppId
$iefConf.ExtObjectId = $extensionApp.ObjectId
out-file -FilePath $iefConfPath -inputobject (ConvertTo-Json $iefConf)


###############################Get Graph REST token ##############################################
$token = Get-MsalToken -clientId $settings.script.clientId -redirectUri $settings.script.redirectUri -Tenant $settings.b2cTenant -Scopes $settings.script.scopes

$headers = @{
   'Content-Type' = 'application/json';
   'Authorization' = ("Bearer {0}" -f $token.AccessToken);
}

############### Register apps in B2C ###########################
"Registering {0} webapp" -f $settings.webApp.name
$url = "https://graph.microsoft.com/beta/applications"
$replyUrls = @(
    ("signin-{0}susi-firsttenant" -f $settings.policyPrefix),
    ("signin-{0}susi2" -f $settings.policyPrefix),
    "members/redeem", 
    ("signin-{0}susint" -f $settings.policyPrefix))
$body = @{
    displayName = $settings.webApp.name;
    isFallbackPublicClient = $false;
    web = @{
        redirectUris = foreach($p in $replyUrls){ ("https://{0}.azurewebsites.net/{1}" -f $settings.webApp.name, $p) };
        implicitGrantSettings = @{
            enableAccessTokenIssuance = $true;
            enableIdTokenIssuance = $true
        }
        logoutUrl = "https://{0}.azurewebsites.net/home/signout" -f $settings.webApp.name
    };
    identifierUris = @(("https://{0}/{1}" -f $settings.b2cTenant, $settings.webApp.name));
}
$webAppReg = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body (ConvertTo-Json -Depth 3 $body)
"Registering {0} API app" -f $settings.webAPI.name
$body = @{
    "displayName" = $settings.webAPI.name;
    "isFallbackPublicClient" = $false;
    "web" = @{
        "redirectUris" =  @(("https://{0}.azurewebsites.net" -f $settings.webAPI.name))
        "implicitGrantSettings" = @{
            "enableAccessTokenIssuance" = $false;
            "enableIdTokenIssuance" = $false
        }
    };
    "identifierUris" = @(("https://{0}/{1}" -f $settings.b2cTenant, $settings.webAPI.name))
}
$webAPIReg = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body (ConvertTo-Json -Depth 3 $body)
#################### Add scopes #########################
"Adding scopes to registered apps"
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
Invoke-RestMethod -Uri $url  -Method Patch -Headers $headers -Body (ConvertTo-Json -Depth 3 $body)

################## Register ServicePrincipals SP #########################
"Register {0} API SP" -f $webAPIReg.displayName
$url = "https://graph.microsoft.com/beta/serviceprincipals"
$body = @{
    "appId" = $webAPIReg.appId;
    "displayName" = $webAPIReg.displayName
}
$webAPISP = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body (ConvertTo-Json $body)
"Register {0} web app SP" -f $webAppReg.displayName
$body = @{
    "appId" = $webAppReg.appId;
    "displayName" = $webAppReg.displayName
}
$webAppSP = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body (ConvertTo-Json $body)
"Find Graph SP and add openid, etc. scopes to the web app"
# The following call was returning all grants. Therefore using PS instead
# $url = "https://graph.microsoft.com/beta/servicePrincipals?$filter=publisherName eq 'Microsoft Graph' and (displayName eq 'Microsoft Graph' or startswith(displayName,'Microsoft Graph'))"
# $url = "https://graph.microsoft.com/beta/servicePrincipals?$filter=displayName eq 'Microsoft Graph'"
# $graphSP = Invoke-RestMethod -Uri $url -Method Get -Headers $headers
$sps = Invoke-RestMethod -Uri "https://graph.microsoft.com/beta/servicePrincipals" -Method Get -Headers $headers
foreach($sp in $sps.value){ 
    if ($sp.appDisplayName -eq 'Microsoft Graph') {
        $graphSP = $sp
        break
    }
}
$today = Get-Date
$body = @{
    "clientId" = $webAppSP.id;
    "consentType" = "AllPrincipals";
    "expiryTime" = (Get-Date -Year ($today.Year + 3) -Format o);
    "principalId" = $null;
    "resourceId" = $graphSP.id
    "scope" = "openid offline_access"
}
Invoke-RestMethod -Uri "https://graph.microsoft.com/beta/oauth2PermissionGrants"  -Method Post -Headers $headers -Body (ConvertTo-Json $body)
##################### Grant permission for web app to call the API #########################
"Add API permissions to the web app"
$body = @{
    "clientId" = $webAppSP.id;
    "consentType" = "AllPrincipals";
    "expiryTime" = (Get-Date -Year ($today.Year + 3) -Format o);
    "principalId" = $null;
    "resourceId" = $webAPISP.id
    "scope" = "Members.ReadAll"
}
Invoke-RestMethod -Uri "https://graph.microsoft.com/beta/oauth2PermissionGrants"  -Method Post -Headers $headers -Body (ConvertTo-Json $body)

################### Add secret key to web app ###################################
"Add secret key to web app"
$body = @{
    "passwordCredentials" = @(
    @{
        displayName = "PS Generated"
        customKeyIdentifier = "ScriptRequest";
        endDateTime = "2299-12-31T00:00:00";
    })
}
$url = "https://graph.microsoft.com/beta/applications/{0}/addPassword" -f $webAppReg.id
$webAppCreds = Invoke-RestMethod -Uri $url  -Method Post -Headers $headers -Body (ConvertTo-Json -Depth 3 $body)

############### DONE updating B2C ########################################

##########################################################################
$svcPlan = $settings.webApp.name
#$webAppSvc = "{0}$(Get-Random)" -f ($settings.webApp.name)
#$webAPISvc = "{0}$(Get-Random)" -f ($settings.webAPI.name)
$webAppSvc = $settings.webApp.name
$webAPISvc = $settings.webAPI.name
"Deploying apps to {0}" -f $azure.Name
"Configuring {0} B2C tenant" -f $b2c.TenantDomain

#########################################################################
"Registering AAD (client credentials) REST API app and service principal"
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
"Creating Azure Resource Group, Service Plan and Web Apps (REST and UI)"
New-AzResourceGroup -Name $settings.resourceGroup -location $settings.location
New-AzAppServicePlan -Name $svcPlan -location $settings.location -ResourceGroupName $settings.resourceGroup -Tier B1
# Web APP and Web API
$app = New-AzWebApp -Name $webAppSvc `
    -location $settings.location `
    -AppServicePlan $svcPlan `
    -ResourceGroupName $settings.resourceGroup -HttpLoggingEnabled $true

$props = @{
    "AzureAD:TenantId" = $b2c.TenantId.ToString();
    "AzureAD:ClientId" = $webAppReg.appId.ToString();
    "AzureAD:ClientSecret" = $webAppCreds.secretText;
    "AzureAD:Domain" = $b2c.TenantDomain;
    RestUrl = "https://{0}.azurewebsites.net" -f $webAPISvc;
    RestApp = $settings.webAPI.name;
    AllowedHosts = "*";
    PolicyPrefix = $settings.policyPrefix
}
Set-AzWebApp -Name $webAppSvc `
    -ResourceGroupName $settings.resourceGroup `
    -AppSettings $props -HttpLoggingEnabled $true

####################### Create X509 cert for authn to REST #######################################
"Creating X509 cert for IEF to Oauth2 autz"
$certSubject = ("CN={0}.{1}" -f $settings.webApi.name, $b2c.TenantDomain)
Write-Host ("Creating X509 cert for IEF policy authentication to allow REST calls")

$certSubject = ("CN={0}.{1}" -f $settings.webApi.name, $b2c.TenantDomain)

$cert = New-SelfSignedCertificate `
    -KeyExportPolicy Exportable `
    -Subject ($certSubject) `
    -KeyAlgorithm RSA `
    -KeyLength 2048 `
    -KeyUsage DigitalSignature `
    -NotAfter (Get-Date).AddMonths(12) `
    -CertStoreLocation "Cert:\CurrentUser\My"
$pfxPwdPlain = "password"
$pfxPwd = ConvertTo-SecureString -String $pfxPwdPlain -Force -AsPlainText
#Export-Certificate -Cert $cert -FilePath $certPath
$pfxPath = ".\RESTClientCert.pfx"
$cert | Export-PfxCertificate -FilePath $pfxPath -Password $pfxPwd
$pkcs12=[Convert]::ToBase64String([System.IO.File]::ReadAllBytes((get-childitem -path $pfxPath).FullName))
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
    password = $pfxPwdPlain
}
$key = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body (ConvertTo-Json $body)


##########################  Create Azure Web Apps services for REST API app ##############################
"Creating Azure Web Apps services for REST API app"
$api = New-AzWebApp -Name $webAPISvc `
    -location $settings.location `
    -AppServicePlan $svcPlan `
    -ResourceGroupName $settings.resourceGroup  `
    -ErrorAction stop
Add-Type -AssemblyName System.Security
# TODO: use B2C to generate a key rather
[Reflection.Assembly]::LoadWithPartialName("System.Security")
$rijndael = new-Object System.Security.Cryptography.RijndaelManaged
$rijndael.GenerateKey()
$invitationSecret = ([Convert]::ToBase64String($rijndael.Key))
$rijndael.Dispose()
UploadIEFSymKey -keysetName "InvitationTokenSigningKey" -value $invitationSecret
$props = @{
    "AuthCert:thumbprint" = $cert.Thumbprint;
    "AuthCert:issuer" = $certSubject;
    "AuthCert:subject" = $certSubject;
    "B2C:TenantName" = $b2c.TenantDomain.Split('.')[0];
    "B2C:TenantId" = $b2c.TenantId.ToString();
    "B2C:ClientId" = $webAPIReg.appId.ToString();
    "B2C:Policy" = ("b2c_1a_{0}susi2" -f $settings.policyPrefix);
    "ClientCreds:Instance" = "https://login.microsoftonline.com/";
    "ClientCreds:TenantId" = $b2c.TenantId.ToString();
    "ClientCreds:ClientId" = $ccredsApp.appId;
    "ClientCreds:ClientSecret" = $ccredPwd.Value;
    "ClientCreds:RedirectUri" = ("https://{0}.azurewebsites.com" -f $settings.webAPI.name)
    "Invitation:InvitationPolicy" = ("B2C_1A_{0}Invitation" -f $settings.policyPrefix);
    "Invitation:ValidityMinutes" = "360";
    "Invitation:RedeemReplyUrl" = "members/redeem";
    "Invitation:SigningKey" = $invitationSecret; 
    AllowedHosts = "*";
    WEBSITE_LOAD_CERTIFICATES = $cert.Thumbprint;
    WEBSITE_NODE_DEFAULT_VERSION = "6.9.1";
}
Set-AzWebApp -Name $webAPISvc `
        -ResourceGroupName $settings.resourceGroup `
        -AppSettings $props -HttpLoggingEnabled $true
$api.ClientCertEnabled = $true
$api.ClientCertExclusionPaths = "/tenant/oauth2"
Set-AzWebApp -WebApp $api

#New-AzWebAppSSLBinding -WebAppName $webAPISvc `
#        -ResourceGroupName $settings.resourceGroup `
#        -Name 'IEFRest' `
#        -CertificateFilePath $certPath

$iefConf.RestTenantCreate = ("https://{0}/tenant" -f $api.DefaultHostName)
$iefConf.RestGetOrJoinTenant = ("https://{0}/tenant/member" -f $api.DefaultHostName)
$iefConf.RestGetTenantForUser = ("https://{0}/tenant/currmember" -f $api.DefaultHostName)
$iefConf.RestGetFirstTenant = ("https://{0}/tenant/first" -f $api.DefaultHostName)
#$iefConf | Add-Member -MemberType NoteProperty -Name 'RestGetFirstTenant' -Value ("https://{0}/tenant/first" -f $api.DefaultHostName)
out-file -FilePath $iefConfPath -inputobject (ConvertTo-Json $iefConf)


########################## Create AADCommon for work accounts signin ##################
"Registering AAD Common app to support signin with work address using AD multi-tenant support"
$aadCommon = Get-AzureADApplication -filter "displayName eq 'AADCommon'"
if ($aadCommon.Count -eq 0) {
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
        -ReplyUrls @( ("https://{0}.b2clogin.com/{1}/oauth2/authresp" -f $b2c.TenantDomain.Split('.')[0], $b2c.TenantId))
} else {
    # in case there is more than one
    $aadCommon = $aadCommon[0]
}
$aadCommonPwd = New-AzureADApplicationPasswordCredential -ObjectId $aadCommon.objectId -CustomKeyIdentifier "PS Generated"
$appCommonSP = New-AzureADServicePrincipal -AppId $aadCommon.appId -AccountEnabled $true

UploadIEFSymKey -keysetName "AADCommonSecret" -value $aadCommonPwd.Value
$iefConf.AADCommonAppId = $aadCommon.appId
$iefConf.AADCommonSecret = "B2C_1A_AADCommonSecret"
out-file -FilePath $iefConfPath -inputobject (ConvertTo-Json $iefConf)


############################### Deploy code ############################################

Upload-IEFPolicies

#$props = @{
#    token = $settings.webApp.gitToken;
#}
#Set-AzResource -PropertyObject $props `
#-ResourceId /providers/Microsoft.Web/sourcecontrols/GitHub -ApiVersion 2015-08-01 -Force

# Do not update too soon
"Deploying apps from github"
Start-sleep -s 30

# Configure GitHub deployment from your GitHub repo and deploy once.
# Web App
$props = @{
    repoUrl = $settings.webApp.gitRepo;
    branch = "master";
    isManualIntegration = "true";
}
Set-AzResource -PropertyObject $props -ResourceGroupName $settings.resourceGroup `
    -ResourceType Microsoft.Web/sites/sourcecontrols -ResourceName ("{0}/web" -f $webAppSvc) `
    -ApiVersion 2015-08-01 -Force
# WebAPI
$props = @{
    repoUrl = $settings.webAPI.gitRepo;
    branch = "master";
    isManualIntegration = "true";
    #build = "kudu";
}
Set-AzResource -PropertyObject $props -ResourceGroupName $settings.resourceGroup `
    -ResourceType Microsoft.Web/sites/sourcecontrols -ResourceName ("{0}/web" -f $webAPISvc) `
    -ApiVersion 2015-08-01 -Force


# "Use the App Registration (Preview) B2C blade to modify the demo app registration to include Microsoft Graph openid and offline_access permissions"
"Please use the Azure portal or the following url to grant admin consent to permissions needed by the {0}-clientcreds application in your N2C tenant" -f $webAppSvc
"https://login.microsoftonline.com/{0}/oauth2/authorize?client_id={1}&scope=openid%20offline_access&response_type=code&response_mode=form_post&nonce=123" -f $b2c.TenantDomain, $ccredsApp.appId

$InternetExplorer=new-object -com internetexplorer.application
$InternetExplorer.navigate2(("https://login.microsoftonline.com/{0}/oauth2/authorize?client_id={1}&scope=openid%20offline_access&response_type=code&response_mode=form_post&nonce=123" -f $b2c.TenantDomain, $ccredsApp.appId))
$InternetExplorer.visible=$true
