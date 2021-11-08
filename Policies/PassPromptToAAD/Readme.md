# Require new login from federated AAD on b2C signin

## Purpose
Passes the *prompt=...* parameter used to initiate the B2C login to a federated AzureAD. Useful if there is a requirement that a new signin to B2C should **not** use SSO to silently use an existing session the user may already have with the federated AAD tenant.

## Adding sample to your policy set

Use [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies) with the following command

```PowerShell
Add-IefPoliciesSample AppRoles -owner mrochon -repo b2csamples
```
The PassPromptSUSI.json file contains additional parameters you will need to add to your own coonf.json. These paramters are needed to configure the policy for [B2C federation with an AAD tenant](https://docs.microsoft.com/en-us/azure/active-directory-b2c/identity-provider-azure-ad-single-tenant?pivots=b2c-custom-policy).