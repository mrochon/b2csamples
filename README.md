# My B2C samples

## Changes
| Date | Change |
|---|---|
| Sep 2021 | New: Federate B2C as IdP for AAD (Direct Federation) |
| Sep 2021 | New: JIT Migration |
| Sep 2021 | Change: Simplified Invitation sample |
| Oct 2021 | Change: Added PS script to assign group to app role in B2C (AppRoles sample) |
| Oct 2021 | Change: Invitation sample supports local-only or federated-only accounts |
| Oct 2021 | New: Conditional Access |
| Nov 2021 | New: Persisted custom attribute |
| Dec 2021 | New: Optionally, allow profile edit during signin |
| Dec 2021 | Change: Multitenant sample now uses a new SPA app and updated policies and REST functions |
| Feb 2022 | New: Use AAD userinfo endpoint to get user's email address (in case AAD does not return it in the id_token) |
| Feb 2022 | New: Claims encryption |
| Mar 2022 | New: Step up MFA |
| Mar 2022 | Fixed: Refresh token |

## Samples list

| Name  | Description  |
|---|---|
| [AllInOne](https://github.com/mrochon/b2csamples/tree/master/Policies/AllInOne)  | Allow profile edit during signin or password reset |
| [AppRoles](https://github.com/mrochon/b2csamples/tree/master/Policies/AppRoles)  | Support for application roles using standard AAD features |
| [B2CSendOTPWithO365](https://github.com/mrochon/b2csamples/tree/master/Policies/b2cSendOtpWith0365)  | Send email OTP using O365 |
| [CheckEmail](https://github.com/mrochon/b2csamples/tree/master/Policies/CheckEmail)  | Prevents users from signing up or in using emails with specific email domains |
| [ConditionalAccess](https://github.com/mrochon/b2csamples/tree/master/Policies/ConditionalAccess)  | Prevents users from signing up or in using emails with specific email domains |
[Claims encryption](https://github.com/mrochon/b2csamples/tree/master/Policies/ClaimsEncryption)  | Supports encryption/decryption of claims in a token |
| [Custom, persisted attribute](https://github.com/mrochon/b2csamples/tree/master/Policies/PersistCustomAttr)  | Modifies starter pack to add support for a new, persisted custom user attribute |
| [EmailOrUserId](https://github.com/mrochon/b2csamples/tree/master/Policies/EmailAndUserId)  | Allow users to signup with both an email and a user id and user either to signin later on |
  [EmailOrPhoneMFA](https://github.com/mrochon/b2csamples/tree/master/Policies/EmailOrPhoneMFA)  | Allows local users to use either their email or phone for 2nd FA |
| [ForceADWhenAvaialble](https://github.com/mrochon/b2csamples/tree/master/Policies/ForceAADwhenAvailable)  | Users who signup with an email address supported by an AAD tenant will be automatically redirected there (rather than defining local password in B2C) |
| [IdTokenSelfHint](https://github.com/mrochon/b2csamples/tree/master/Policies/IdTokenSelfHint)  | Allows long-running native apps to initiate profile edit without needing to re-authenticate user |
| [Invite](https://github.com/mrochon/b2csamples/tree/master/Policies/Invitation)  | Create/use an invitation link using client_assertion request |
| [JIT Migrate](https://github.com/mrochon/b2csamples/tree/master/Policies/JitMigrate)  | Migrate users using an API to verify their legacy passwords |
| [MultiTenant](https://github.com/mrochon/b2csamples/tree/master/Policies/MultiTenant)  | Supports use of a single B2C tenant to support a muli-tenant SaaS application |
| [PromptForToAAD](https://github.com/mrochon/b2csamples/tree/master/Policies/PromptForToAAD)  | Passes whatever prompt parameter was used with B2C to a federated AAD. |
| [RefreshToken](https://github.com/mrochon/b2csamples/tree/master/Policies/RefreshToken)  | Rejects refresh token exchange if user requested its revocation |
| [SamlIdP](https://github.com/mrochon/b2csamples/tree/master/Policies/SAMLIdP)  | Invite B2C users as B2B users in an Azure AD |
| [Step up MFA](https://github.com/mrochon/b2csamples/tree/master/Policies/StepUpMFA)  | Require MFA even if recently executed |
| [UseUserInfoforEmailClaim](https://github.com/mrochon/b2csamples/tree/master/Policies/UseUserInfoforEmailClaim)  | Invite B2C users as B2B users in an Azure AD |


