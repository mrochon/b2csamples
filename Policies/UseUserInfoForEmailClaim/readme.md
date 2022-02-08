# Use AAD UserInfo for email claim

Policy is based on [ForceAADWhenAvailable](ForceAADWhenAvailable). Additionally it gets the AAD user id_token and then uses it with a REST function to get user's email from the AAD UserInfo endpoint (email address may not be returned in the id_token, depending on AAD setup).


## Adding sample to your policy set

Use [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies) with the following command

```PowerShell
Add-IefPoliciesSample UseUserInfoforEmailClaim -owner mrochon -repo b2csamples
```