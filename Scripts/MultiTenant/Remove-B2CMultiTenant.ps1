# Remove MT

$settings = Get-Content -Path $conf -ErrorAction Stop | Out-String | ConvertFrom-Json

################### Functions ######################################
function RemoveIEFSymKey([string]$keysetName)
{
    $url = ("https://graph.microsoft.com/beta/trustFramework/keySets/{0}" -f $keysetName)
    try {
        Invoke-RestMethod -Uri $url -Method Delete -Headers $headers -ErrorAction Ignore
    } catch {
        $err = $_
    }
    ("Keyset {0} deleted" -f $keysetName)
}
function RemoveAADApp([string] $appName)
{
    $app = Get-AzureADApplication -filter ("displayName eq '{0}'" -f $appName)
    # $app could be a list of apps!!!
    if ($app -ne $null) {
        Remove-AzureADApplication -ObjectId $app.objectId
        ("App {0} deleted" -f $appName)
    }
}

################### Login to AAD and Az ##################################
Write-Host ("Login to Azure with an account with sufficient privilege to create a resource group and web apps")
Connect-AzAccount -ErrorAction Stop
Write-Host ("Login to your B2C directory with an account with sufficient privileges to register applications")
Connect-AzureAD -TenantId $settings.b2cTenant -ErrorAction Stop
$b2c = Get-AzureADCurrentSessionInfo -ErrorAction stop
$azure = Get-AzContext -ErrorAction stop
#########################################################################

RemoveAADApp -appName $settings.webApp.name
RemoveAADApp -appName $settings.webAPI.name
RemoveAADApp -appName ("{0}-clientcreds" -f $settings.webAPI.name)
RemoveAADApp -appName "AADCommon"

RemoveIEFSymKey -keysetName "B2C_1A_InvitationTokenSigningKey"
RemoveIEFSymKey -keysetName "B2C_1A_AADCommonSecret"
RemoveIEFSymKey -keysetName "B2C_1A_RESTClientCert"

Remove-AzResourceGroup($settings.resourceGroup)

Remove-Item ('.\{0}.*' -f $settings.webApp.name)
