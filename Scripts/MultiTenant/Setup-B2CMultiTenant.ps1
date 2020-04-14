$conf = ".\appsettings.json"

$settings = Get-Content -Path $conf -ErrorAction Stop | Out-String | ConvertFrom-Json

################### Login to AAD and Az ##################################
Write-Host ("Login to Azure with an account with sufficeint privilege to create a resource group and web apps")
Connect-AzAccount -ErrorAction Stop
Write-Host ("Login to your B2C directory with an account with sufficient privileges to register applications")
Connect-AzureAD -TenantId ("{0}.onmicrosoft.com" -f $settings.b2cTenant) -ErrorAction Stop

##########################################################################

$svcPlan = $settings.webApp.name
$webAppSvc = "{0}$(Get-Random)" -f ($settings.webApp.name)
$webAPISvc = "{0}$(Get-Random)" -f ($settings.webAPI.name)
$b2c = Get-AzureADCurrentSessionInfo -ErrorAction stop
$azure = Get-AzContext -ErrorAction stop

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
    -IdentifierUris "app://b2crestapi" `
    -DisplayName $settings.webAPI.name `
    -RequiredResourceAccess $access `
    -Oauth2AllowImplicitFlow $false 
$apiPwd = New-AzureADApplicationPasswordCredential -ObjectId $apiApp.objectId -CustomKeyIdentifier "PS Generated"
$appSP = New-AzureADServicePrincipal -AppId $apiApp.appId -AccountEnabled $true
############################################################################


#########################################################################
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
    "AzureAD:ClientId" = $settings.webApp.appId.ToString();
    "AzureAD:ClientSecret" = $settings.webApp.secret;
    "AzureAD:Domain" = $b2c.TenantDomain;
    "Invitation:Domain" = $b2c.TenantDomain;
    "Invitation:ClientId" = $settings.webApp.appId.ToString();
    "Invitation:InvitationPolicy" = "B2C_1A_MTInvitation";
    "Invitation:Issuer" = "b2cmultitenant";
    "Invitation:Audience" = "b2cmultitenant";
    "Invitation:ValidityHours" = "72";
    "Invitation:RedirectPath" = "members/redeem";
    "Invitation:SigningKey" = $settings.InvitationKey;
    RestUrl = "https://{0}.azurewebsites.net" -f $webAPISvc;
    AllowedHosts = "*";
}
Set-AzWebApp -Name $webAppSvc `
    -ResourceGroupName $settings.resourceGroup `
    -AppSettings $props
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
    "B2C:ClientId" = $settings.webAPI.appId.ToString();
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
