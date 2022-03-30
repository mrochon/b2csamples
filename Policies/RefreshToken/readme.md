# Refresh token revocation
Implements support for refresh token revocation in custom policies.

## Based on
Socal starter pack

## Notes

Refresh tokens may be revoked by using [Revoke-AzureADUserAllRefreshToken](https://docs.microsoft.com/en-us/powershell/module/azuread/revoke-azureaduserallrefreshtoken?view=azureadps-2.0)
 PowerShell command, user password change (local user only) or via the portal (User->Revoke sessions).

 ## Adding sample to your policy set

Use [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies) with the following command

```PowerShell
Add-IefPoliciesSample RefreshToken -owner mrochon -repo b2csamples
```