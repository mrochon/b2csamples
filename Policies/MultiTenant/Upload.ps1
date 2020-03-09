# connect-azuread -tenantid b2cmultitenant.onmicrosoft.com
$source = '.'
$conf = '.\settings.json'
$dest = '.\generated'
Upload-IEFPolicies -sourceDirectory $source -configurationFilePath $conf -updatedSourceDirectory $dest -prefix 'MT' 
