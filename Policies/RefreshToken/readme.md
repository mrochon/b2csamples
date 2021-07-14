# Refresh token revocation
Implements support for refresh token revocation in custom policies.

Refresh tokens may be revoked by using [Revoke-AzureADUserAllRefreshToken](https://docs.microsoft.com/en-us/powershell/module/azuread/revoke-azureaduserallrefreshtoken?view=azureadps-2.0)
 PowerShell command, user password change (local user only) or via the portal.