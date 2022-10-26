# Purpose

Based on the [DisplayClaims SocialAndLocalAccount starter pack](https://github.com/Azure-Samples/active-directory-b2c-custom-policy-starterpack/tree/main/Display%20Controls%20Starterpack/SocialAndLocalAccounts), this journey supports user sign-in with an additional step using One Time Password as 2nd factor authentication.

For local account, the journey will attempt to use the *signInName.email* to send the OTP. For federated accounts, either the incomming IdP token has to include the *email* claim or the email needs to be stored in that user's *otherMails* property as the first instance.

Coincidentally, the sample also shows a way to use the use the sign in email as display-only value in the OTP DisplayControl. That is frequently an issue in similar journeys: the copied sign in name does not show up in the the subsequent steps.

## Adding sample to your policy set

Use [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies) to add this sample to the above starter pack:

```PowerShell
Add-IefPoliciesSample EmailMFA -owner mrochon -repo b2csamples
```
