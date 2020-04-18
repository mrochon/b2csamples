# once only: Install-Module -Name MSAL.PS, AzureAD-preview

$conf = ".\appsettings.json"
$settings = Get-Content -Path $conf -ErrorAction Stop | Out-String | ConvertFrom-Json

# Add check whether the app names are stil available

###############################Get Graph REST token ##############################################
$token = Get-MsalToken -clientId $settings.script.clientId -redirectUri $settings.script.redirectUri -Tenant $settings.b2cTenant -Scopes $settings.script.scopes

$headers = @{
   'Content-Type' = 'application/json';
   'Authorization' = ("Bearer {0}" -f $token.AccessToken);
}

############### Register apps in B2C ###########################
$url = "https://graph.microsoft.com/beta/applications"
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
$body.displayName = $settings.webAPI.name
$body.web.redirectUris = @(("https://{0}.com" -f $settings.webAPI.name))
$body.identifierUris = @(("https://{0}/b2crestapi" -f $settings.b2cTenant))
$webAPIReg = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body (ConvertTo-Json -Depth 3 $body)

################## Register ServicePrincipals SP #########################
$body = @{
  "appId" = $webAppReg.appId;
  "displayName" = $webAppReg.displayName
}
$webAppSP = Invoke-RestMethod -Uri "https://graph.microsoft.com/beta/serviceprincipals"  -Method Post -Headers $headers -Body (ConvertTo-Json $body)
$body = @{
  "appId" = $webAPIReg.appId;
  "displayName" = $webAPIReg.displayName
}
$webAPISP = Invoke-RestMethod -Uri "https://graph.microsoft.com/beta/serviceprincipals"  -Method Post -Headers $headers -Body (ConvertTo-Json $body)

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

################### Add secret key ###################################
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
### store in B2C


################### Login to AAD and Az ##################################
Write-Host ("Login to Azure with an account with sufficeint privilege to create a resource group and web apps")
Connect-AzAccount -ErrorAction Stop
Write-Host ("Login to your B2C directory with an account with sufficient privileges to register applications")
Connect-AzureAD -TenantId $settings.b2cTenant -ErrorAction Stop
$b2c = Get-AzureADCurrentSessionInfo -ErrorAction stop
$azure = Get-AzContext -ErrorAction stop

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
$apiApp = New-AzureADApplication `
    -IdentifierUris ("app://{0}" -f $settings.webAPI.name) `
    -DisplayName ("{0}-clientcreds" -f $settings.webAPI.name) `
    -RequiredResourceAccess $access `
    -Oauth2AllowImplicitFlow $false 
$apiPwd = New-AzureADApplicationPasswordCredential -ObjectId $apiApp.objectId -CustomKeyIdentifier "PS Generated"
$appSP = New-AzureADServicePrincipal -AppId $apiApp.appId -AccountEnabled $true


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
Write-Host ("Creating X509 cert for IEF policy authentication to allow REST calls")
Write-Host ("   Certificate id=CN=b2cmtrest")
$cert = New-SelfSignedCertificate `
    -KeyExportPolicy Exportable `
    -Subject "CN=b2cmtrest" `
    -KeyAlgorithm RSA `
    -KeyLength 2048 `
    -KeyUsage DigitalSignature `
    -NotAfter (Get-Date).AddMonths(12) `
    -CertStoreLocation "Cert:\CurrentUser\My"
$certPwd = ConvertTo-SecureString -String $settings.X509KeyPassword -Force -AsPlainText
Get-ChildItem -Path ("cert:\CurrentUser\My\{0}" -f $cert.Thumbprint) | Export-PfxCertificate -FilePath .\b2cmtrest.pfx -Password $certPwd
Export-Certificate -Cert $cert -FilePath .\b2cmtrest.cer

##########################  Create Azure Web Apps for demo app and REST API app ##############################
$apiApp = New-AzureADApplication -DisplayName $settings.WebApi.name

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
    "ClientCreds:ClientId" = $apiApp.appId;
    "ClientCreds:ClientSecret" = $apiPwd.Value;
    AllowedHosts = "*";
    WEBSITE_LOAD_CERTIFICATES = $cert.Thumbprint;
    WEBSITE_NODE_DEFAULT_VERSION = "6.9.1";
}
Set-AzWebApp -Name $webAPISvc `
    -ResourceGroupName $settings.resourceGroup `
    -AppSettings $props
###
###  Add clientcertificatesenabled and which path to skip
###

# Deploy code
$props = @{
    token = $settings.webApp.gitToken;
}
Set-AzResource -PropertyObject $props `
-ResourceId /providers/Microsoft.Web/sourcecontrols/GitHub -ApiVersion 2015-08-01 -Force

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

# Get consent for REST API