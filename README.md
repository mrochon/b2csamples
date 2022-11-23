# My B2C samples

## Dev environment setup

### Software tools
- [Visual Studio Code](https://code.visualstudio.com/Download). **Recommendation:** Set *File->Auto Save* to *On* in Visual Studio Code
- [B2C initial setup](https://aka.ms/b2csetup) or use Initialize-IefPolicies command
- [B2C Extensions](https://marketplace.visualstudio.com/items?itemName=AzureADB2CTools.aadb2c)
- [PowerShell 7.x](https://learn.microsoft.com/en-us/shows/it-ops-talk/how-to-install-powershell-7)
- [IefPolicies](https://www.powershellgallery.com/packages/IefPolicies/), [documentation](https://github.com/mrochon/IEFPolicies)

### Dev B2C setup
- Register a web app (Token Viewer) with reply url *https://oidcdebugger.com/debug*; allow return of the access and id token in the Authentication tab
- Create a user (*Users->New user->Create user*) with B2C's upn, e.g. *someuser@myb2c.onmicrosoft.com* - useful for experiemnting with MS Graph through the [Graph Explorer](https://aka.ms/ge).

### Usage example

1. Open VSCode
2. Select *Terminal->New Terminal*
3. Ensure your PowerShell terminal is using PS 7.x (*$host.Version*)
4. Create a new folder and change to it (e.g. *mkdir myProject; cd myProject*)
4. Enter *New-IefPolicies*
5. Select a starter pack, e.g. *SL* (Social and local accounts)
6. Enter *Connect-IefPolicies <your b2c name; onmicrosoft.com not needed>*
7. Followe displayed instructions to sign in
8. Enter *Import-IefPolicies*
9. The downloaded starter pack will be modified for use in your B2C and uploaded (you can see the modified files in the ./debug folder)
10. Use https://portal.azure.com B2C menus to execute your policies
11. Repeating *import-iefpolicies* will upload policies modified since the last import and any policies depending on it, e.g. modifying the TrustFrameworkBase.xml policy will result in import of all policies since they are all based on that file.


## Samples list

| Name  | Description  |
|---|---|
| [AllInOne](https://github.com/mrochon/b2csamples/tree/master/Policies/AllInOne)  | Allow profile edit during signin or password reset |
| [AppRoles](https://github.com/mrochon/b2csamples/tree/master/Policies/AppRoles)  | Support for application roles using standard AAD features |
| [Batch migration](https://github.com/mrochon/b2csamples/tree/master/Policies/BatchMigration)  | Batch user creation with email to reset pwd using login_hint |
| [B2CSendOTPWithO365](https://github.com/mrochon/b2csamples/tree/master/Policies/b2cSendOtpWith0365)  | Send email OTP using O365 |
| [CheckEmail](https://github.com/mrochon/b2csamples/tree/master/Policies/CheckEmail)  | Prevents users from signing up or in using emails with specific email domains |
| [Choose 2nd FA](https://github.com/mrochon/b2csamples/tree/master/Policies/MFAChoice)  | User can choose whether to use enail OTP, phone OTP/sms/call or MS Authenticator TOTP |
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
| Jun 2022 | New: batch migration |
| Nov 2022 | New: user choice of 2nd FA |
