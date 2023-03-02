# Force use of configured IdPs 

Requires federation with [AAD with support for multi-tenant signin](https://docs.microsoft.com/en-us/azure/active-directory-b2c/identity-provider-azure-ad-multi-tenant?pivots=b2c-custom-policy).

Sign-in policy which first asks for user email, determines whether there is an existing Azure AD tenant or
some other configured IdP. If one exists is redirects the user to that IdP. Other users are sent to local signin.

The REST function used to determine the user's Home Realm is publicly accessible and referenced in the conf.json file.

## Deployment

Use [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies) with the following command

```PowerShell
Add-IefPoliciesSample HRD -owner mrochon -repo b2csamples
```
