# Choose MFA type

Allows the user to choose the 2nd FA to use:

- Email OTP
- Phone OTP
- Microsoft Authenticator TOTP

Based on LocalAccounts starterpack.

## Adding sample to your policy set

Use [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies) with the following command:

```PowerShell
Add-IefPoliciesSample MFAChoice -owner mrochon -repo b2csamples
```

