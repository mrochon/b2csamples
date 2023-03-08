# Force use of configured IdPs 

Sign-in policy with built-in Home Realm Discovery.

Based on SocialAndLocal starter pack

## Operation

The SignInSignUp journey redirects the user to the appropriate IdP (sample supports any Azure AD or MSA account) or allows the user
to signin using a local account.

If no hints are provided in the initial token requests, the journey will ask the user for their email and call a REST function which uses
the user domain to check whether there is an existing Azure AD with that domain or whether the domain represents an MSA account. The Rest function can be extended to support other IdPs (e.g. gmail). If no appropriate IdP is found the user is asked to sign in using local password.

The requesting application can also specify the domain_hint parameter. If specified, and the journey includes TechnicalProfiles corresponding to
the value of the hint, that IdP will bbe chose. Note that in this sample the two valid domain_hint values are *aad* and *msa*. If the domain_hint has the value of *aad* **and** the request also contains an additional query parameter **aadDomain**, its value will be used to redirect the user to that AAD tenant.

If the request includes a *login_hint*, its value will be useed to make the home realm discovery.

The REST function used to determine the user's Home Realm is publicly accessible and referenced in the conf.json file.

## Deployment

Use [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies) with the following command

```PowerShell
Add-IefPoliciesSample HRD -owner mrochon -repo b2csamples
```

This sample uses federation with [AAD with support for multi-tenant signin](https://docs.microsoft.com/en-us/azure/active-directory-b2c/identity-provider-azure-ad-multi-tenant?pivots=b2c-custom-policy). To add one to your policy set you can use

```PowerShell
Add-IefPoliciesIdP AAD -name WORK
```
(since *WORK* is already referenced in the *conf* file included with this sample, you only need to take the client id value in the ./federations/*.json file
- after executing the above command - and repalce the one listed in the conf file in this sample)