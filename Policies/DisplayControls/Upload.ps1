# connect-azuread -tenantid mrochonb2cprod.onmicrosoft.com
$source = 'C:\Users\mrochon\source\repos\b2csamples\Policies\DisplayControls'
$conf = 'C:\Users\mrochon\source\repos\b2csamples\Policies\DisplayControls\conf.json'
$dest = 'C:\Users\mrochon\source\repos\b2csamples\Policies\DisplayControls\uploaded'
Upload-IEFPolicies -sourceDirectory $source -configurationFilePath $conf -updatedSourceDirectory $dest -prefix 'DC' 