# Journeys embedding pasword reset as an option
Avoid having to create a separate Password Reset journey as per this documentation.

(Currently only Sign In/Sign up provided).

## Setup
Based on SocialAndLocal starter packs. when using other packs a merge of their journeys with the ones here is required.

## Adding to your policy set

Use [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies) with the following command

```PowerShell
Add-IefPoliciesSample SUSI_Pwd_Reset -owner mrochon -repo b2csamples
```
