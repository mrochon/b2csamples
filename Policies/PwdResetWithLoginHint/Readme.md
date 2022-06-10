# Password reset with login_hint

Journey to support batch user migrations. After creating user data in B2C, uusers could be sent to this journey via an email link. The link would include the user's email as *login_hint*. The user would then use the journey to initiate OTP authentication and be able to set the password. The journey does not allow the user to change the email (of course, the value of the login_hint may be modified in the url itself so this is more of convenience then security feature).

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
Add-IefPoliciesSample PwdResetWithLoginHint -owner mrochon -repo b2csamples
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
