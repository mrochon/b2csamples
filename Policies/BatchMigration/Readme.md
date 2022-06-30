# Batch migration with email-initiated password reset

Application creates b2C users using a csv file as input. Users are created with a random password and sent an email (using SendGrid) with link to a pasword
reset policy. The policy expects to get user's email address in the *login_hint* parameter and does not allow the user to change it in the UI (though of course, the user could have manipulated the login_hint itself so this is more of convenience than security feature).

## Adding sample to your policy set

Use [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies) with the following commands:

To download a starter pack to the current folder:
```PowerShell
Connect-IefPolicies yourtenantname
New-IefPolicies
```
After replacing *yourtenantname* with the name of your B2C tenant (the *.onmicrosoft.com* suffix is not needed)

To add this sample to the above starter pack
```PowerShell
Add-IefPoliciesSample BatchMigration -owner mrochon -repo b2csamples
```

To import to your tenant:
```PowerShell
Import-IefPolicies
```

To remove these policies from your tenant:
```PowerShell
Remove-IefPolicies V1
```
After replacing *'V1'* with the value of the Prefix attribute in the *conf.json* file
