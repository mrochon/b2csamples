# Purpose

Based on the [DisplayClaims LocalAccount starter pack](https://github.com/Azure-Samples/active-directory-b2c-custom-policy-starterpack/tree/main/Display%20Controls%20Starterpack/LocalAccounts), this journey supports user sign-in with an additional step using One Time Password as 2nd factor authentication.

Coincidentally, the sample also shows a way to use the use the sign in email as display-only value in the OTP DisplayControl. That is frequently an issue in similar journeys: the copied sign in name does not show up in the the subsequent steps.

## Adding sample to your policy set

Use [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies) to add this sample to the above starter pack:

```PowerShell
Add-IefPoliciesSample EmailMFA -owner mrochon -repo b2csamples
```
