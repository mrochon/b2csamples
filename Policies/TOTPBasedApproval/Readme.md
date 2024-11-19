# Use TOTP code from one user to approve actions by another 

Adds a new journey, which when invoked will require value of TOTP from another, designated user to complete. In this scenario:

## Operation

1. A user, Store Manager, is using an application
2. At some stage, the application needs to execute an action requiring another user's (District Manager) approval.
3. Application invokes the StoreManager_TOTP journey passing the objectId of the District Manager as *mg* query parameter in the token request
4. The journey verifies that the District Manager has been configured with an Authenticator and displays UI asking for the TOTP value
5. Store Manager calls the District Manager, who checks their Authenticator and provides the current TOTP to the Store Manager
6. Journey validates the TOTP and issues a token to the Store Manager application confirming approval.

District Manager may use the DistrictManager_TOTP journey to setup their Authenticator.

Based on MFA starter pack.

## Deployment

Use [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies) with the following command

```PowerShell
Add-IefPoliciesSample TOTPBasedApproval -owner mrochon -repo b2csamples
```

This sample uses federation with [AAD with support for multi-tenant signin](https://docs.microsoft.com/en-us/azure/active-directory-b2c/identity-provider-azure-ad-multi-tenant?pivots=b2c-custom-policy). To add one to your policy set you can use

```PowerShell
Add-IefPoliciesIdP AAD -name WORK
```
(copy the client id value from the *./federations/conf.json* file to the conf.json file in this folder)