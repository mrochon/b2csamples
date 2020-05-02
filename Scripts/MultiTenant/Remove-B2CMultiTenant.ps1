# Remove MT

$settings = Get-Content -Path ".\setupSettings.json" -ErrorAction Stop | Out-String | ConvertFrom-Json

################### Functions ######################################
function RemoveIEFSymKey([string]$keysetName)
{
    $url = ("https://graph.microsoft.com/beta/trustFramework/keySets/{0}" -f $keysetName)
    try {
        Invoke-RestMethod -Uri $url -Method Delete -Headers $headers -ErrorAction Ignore
    } catch {
        ("Deletion of keyset {0} failed" -f $keysetName)
        $_
        return
    }
    ("Keyset {0} deleted" -f $keysetName)
}
function RemoveAADApp([string] $appName)
{
    $app = Get-AzureADApplication -filter ("displayName eq '{0}'" -f $appName)
    # $app could be a list of apps!!!
    if ($app -ne $null) {
        try {
            Invoke-RestMethod -Uri ("https://graph.microsoft.com/beta/applications/{0}" -f $app.objectId) `
                -Method Delete -Headers $headers -ErrorAction Ignore
            #Remove-AzureADApplication -ObjectId $app.objectId
        } catch {
            ("App {0} failed to delete" -f $appName)
            _$
            return
        }
        ("App {0} deleted" -f $appName)
    } else {
        ("App {0} not found" -f $appName)
    }
}

################### Login to AAD and Az ##################################
Write-Host (("Login to Azure with an account with sufficient privilege to delete the {0} resource group" -f $settings.resourceGroup))
Connect-AzAccount -ErrorAction Stop
Write-Host ("Login to your B2C directory with an account with sufficient privileges to delete applications & policy keys")
Connect-AzureAD -TenantId $settings.b2cTenant -ErrorAction Stop
$b2c = Get-AzureADCurrentSessionInfo -ErrorAction stop
$azure = Get-AzContext -ErrorAction stop
###############################Get Graph REST token ##############################################
$token = Get-MsalToken -clientId $settings.script.clientId -redirectUri $settings.script.redirectUri -Tenant $settings.b2cTenant -Scopes $settings.script.scopes
$headers = @{
   'Content-Type' = 'application/json';
   'Authorization' = ("Bearer {0}" -f $token.AccessToken);
}
#########################################################################

RemoveAADApp -appName $settings.webApp.name
RemoveAADApp -appName $settings.webAPI.name
RemoveAADApp -appName ("{0}-clientcreds" -f $settings.webAPI.name)
RemoveAADApp -appName $settings.AADCommonAppName

RemoveIEFSymKey -keysetName "B2C_1A_InvitationTokenSigningKey"
RemoveIEFSymKey -keysetName "B2C_1A_AADCommonSecret"
RemoveIEFSymKey -keysetName "B2C_1A_RESTClientCert"

Remove-AzResourceGroup($settings.resourceGroup)

Remove-Item '.\RESTClientCert.cer'
Remove-Item '.\RESTClientCert.pfx'