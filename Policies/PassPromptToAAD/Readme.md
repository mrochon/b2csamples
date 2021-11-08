# Pass prompt login parameter to a federated AAD

## Purpose
Passes the *prompt=...* parameter used to initiate the B2C login to a federated AzureAD. Useful, for example if there is a requirement that a new signin to B2C should **not** re-use existing AAD sessions. Note, that this approach maynot work with other federated IdPs - some of them do not support OIDC at all or do not support the OIDC prompt parameter.

## Adding sample to your policy set
May be added to any policy set based, which includes social login support.

To add to your existing policy set [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies) with the following command

```PowerShell
Add-IefPoliciesSample PassPromptToAAD -owner mrochon -repo b2csamples
```
The PassPromptToAAD.json file contains additional parameters you will need to add to your own coonf.json. These paramters are needed to configure the policy for [B2C federation with an AAD tenant](https://docs.microsoft.com/en-us/azure/active-directory-b2c/identity-provider-azure-ad-single-tenant?pivots=b2c-custom-policy).