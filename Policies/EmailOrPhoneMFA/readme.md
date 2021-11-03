# Use verified email or phone as 2nd FA

Based on the SocialAndLocalWithMFA starter pack.

Signup part of the journey verifies user signin email and records user's MFA phone number.
Signin part gives user the choice of using either their signin email or phone for 2nd FA.

## Adding sample to your policy set

Use [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies) with the following command

```PowerShell
Add-IefPoliciesSample EmailOrPhoneMFA -owner mrochon -repo b2csamples
```