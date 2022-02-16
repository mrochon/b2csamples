# Force use of AAD (work account) when available

Requires federation with [AAD with support for multi-tenant signin](https://docs.microsoft.com/en-us/azure/active-directory-b2c/identity-provider-azure-ad-multi-tenant?pivots=b2c-custom-policy).

Sign-in policy which first asks for user email, determines whether there is an existing Azure AD tenant for
the user's domain and, if it is redirects the user to that tenant. Other users are sent to local signin.

Could be extended to redirect users to other, non-AAD federated IdPs.

The REST function used to determine the user's Home Realm (currently AAD only) is publicly accessible and referenced in the conf.json file.

uses a separate REST TechnicalProfile to call AAD UserInfo endpoint to get user's email, which **may** not be returned in the defaults token.

## Deployment

Use [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies) with the following command

```PowerShell
Add-IefPoliciesSample ForceAADWhenAvailable -owner mrochon -repo b2csamples
```

**Note:** currently the email address used for HRD is not copied to or write-protected when displayed later
in the sign-in dialog, i.e. the user could change it. That's a bug - looking at how to fix it.