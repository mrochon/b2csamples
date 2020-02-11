$conf = 'C:\Users\mrochon\source\repos\b2csamples\Policies\MultiTenant\settings.json'
$source= 'C:\Users\mrochon\source\repos\b2csamples\Policies\MultiTenant'
$dest = 'c:\temp\multi'
Upload-IEFPolicies -configurationFilePath $conf -sourceDirectory $source -updatedSourceDirectory $dest
