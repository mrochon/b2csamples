# Step up MFA

Sign-in only journey using Phone Factor MFA on every invocation. Thanks to AntonS for suggesting this approach (*change session management of the Phone Factor TP to use SM-NOOP*).

Requires MFA starter pack.

## Adding sample to your policy set

Use [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies) with the following commands:

To download a starter pack to the current folder:
```PowerShell
Connect-IefPolicies yourtenantname
New-IefPolicies
```
Replace *yourtenantname* with the name of your B2C tenant (the *.onmicrosoft.com* suffix is not needed).
Use 'M' option to download the MAF starter pack.

To add this sample to the above starter pack
```PowerShell
Add-IefPoliciesSample StepUpMFA -owner mrochon -repo b2csamples
```

To import to your tenant:
```PowerShell
Import-IefPolicies
```

To remove these policies from your tenant:
```PowerShell
Remove-IefPolicies prefix
```
Replace *'prefix'* with the value of the Prefix attribute in your *conf.json* file
