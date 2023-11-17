# My B2C samples

## Dev environment setup

### Software tools
- [Visual Studio Code](https://code.visualstudio.com/Download). **Recommendation:** Set *File->Auto Save* to *On* in Visual Studio Code
- [B2C Extensions](https://marketplace.visualstudio.com/items?itemName=AzureADB2CTools.aadb2c)
- An xml handling extension, e.g. [Red Hat Xml](https://marketplace.visualstudio.com/items?itemName=redhat.vscode-xml)
- (Optionally) make sure your VSCode settings.json file includes the following xml file association
```xml
    "xml.fileAssociations": [
        {
          "pattern": "**.xml",
          "systemId": "https://raw.githubusercontent.com/Azure-Samples/active-directory-b2c-custom-policy-starterpack/master/TrustFrameworkPolicy_0.3.0.0.xsd"
        }
      ],
```
- [PowerShell 7.x](https://learn.microsoft.com/en-us/shows/it-ops-talk/how-to-install-powershell-7)
- [IefPolicies](https://www.powershellgallery.com/packages/IefPolicies/), [documentation](https://github.com/mrochon/IEFPolicies)
- [SAML2 test ServiceProvider](https://samltestapp2.azurewebsites.net/). Register an app in your B2C with this apps issuer id and rely url


### GitHub Codespaces
If you have access to Github codespaces, copy the devcontainer folder into the root of your project. This codespace is configured as per above
section.

### Dev B2C setup
- Register a web app (Token Viewer) with reply url *https://oidcdebugger.com/debug*; allow return of the access and id token in the Authentication tab
- Create a user (*Users->New user->Create user*) with B2C's upn, e.g. *someuser@myb2c.onmicrosoft.com* - useful for experiemnting with MS Graph through the [Graph Explorer](https://aka.ms/ge).
- Using *Invite user* to add your corporate users who will manage B2C development. Give them Global Admin privilege (or other needed for their functions in B2C).
- Use [B2C Setup tool](https://aka.ms/b2csetup) to initialize B2C for IEF use.

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
  [CallGraph](https://github.com/mrochon/b2csamples/tree/master/Policies/CallGraph)  | Call a Graph API |
| [CheckEmail](https://github.com/mrochon/b2csamples/tree/master/Policies/CheckEmail)  | Prevents users from signing up or in using emails with specific email domains |
| [Choose 2nd FA](https://github.com/mrochon/b2csamples/tree/master/Policies/MFAChoice)  | User can choose whether to use enail OTP, phone OTP/sms/call or MS Authenticator TOTP |
| [ConditionalAccess](https://github.com/mrochon/b2csamples/tree/master/Policies/ConditionalAccess)  | Prevents users from signing up or in using emails with specific email domains |
| [ContinueOnOTPVerified](https://github.com/mrochon/b2csamples/tree/master/Policies/ContinueOnOTPVerified)  | UI continues to new password screen as soon as OTP is verified. Continue button is not used. |
[Claims encryption](https://github.com/mrochon/b2csamples/tree/master/Policies/ClaimsEncryption)  | Supports encryption/decryption of claims in a token |
| [Custom, persisted attribute](https://github.com/mrochon/b2csamples/tree/master/Policies/PersistCustomAttr)  | Modifies starter pack to add support for a new, persisted custom user attribute |
| [Custom token refresh](https://github.com/mrochon/b2csamples/tree/master/Policies/CustomTokenRefreshExpiryTime)  | Uses REST function to validate token expiry time |
| [EmailOrUserId](https://github.com/mrochon/b2csamples/tree/master/Policies/EmailAndUserId)  | Allow users to signup with both an email and a user id and user either to signin later on |
  [EmailOrPhoneMFA](https://github.com/mrochon/b2csamples/tree/master/Policies/EmailOrPhoneMFA)  | Allows local users to use either their email or phone for 2nd FA |
| [Embedded pwd reset](https://github.com/mrochon/b2csamples/tree/master/Policies/EmbeddedPwdReset)  | Journeys embedding pwd reset functionality as user selectable option |  
| [HRD](https://github.com/mrochon/b2csamples/tree/master/Policies/HRD)  | Redirects user to configured IdP (AAD or other) if user's email domain is supported by a configured IdP |
| [IdTokenSelfHint](https://github.com/mrochon/b2csamples/tree/master/Policies/IdTokenSelfHint)  | Allows long-running native apps to initiate profile edit without needing to re-authenticate user |
| [Invite](https://github.com/mrochon/b2csamples/tree/master/Policies/Invitation)  | Create/use an invitation link using client_assertion request |
| [JIT Migrate](https://github.com/mrochon/b2csamples/tree/master/Policies/JitMigrate)  | Migrate users using an API to verify their legacy passwords |
| [MultiTenant](https://github.com/mrochon/b2csamples/tree/master/Policies/MultiTenant)  | Supports use of a single B2C tenant to support a muli-tenant SaaS application |
| [PromptForToAAD](https://github.com/mrochon/b2csamples/tree/master/Policies/PromptForToAAD)  | Passes whatever prompt parameter was used with B2C to a federated AAD. |
| [RefreshToken](https://github.com/mrochon/b2csamples/tree/master/Policies/RefreshToken)  | Rejects refresh token exchange if user requested its revocation |
| [SamlIdP](https://github.com/mrochon/b2csamples/tree/master/Policies/SAMLIdP)  | Invite B2C users as B2B users in an Azure AD |
| [Step up MFA](https://github.com/mrochon/b2csamples/tree/master/Policies/StepUpMFA)  | Require MFA even if recently executed |
| [TOTP](https://github.com/mrochon/b2csamples/tree/master/Policies/TOTP)  | Add Authenticator/TOTP |
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
| Mar 2023 | New: call Graph |
| Jun 2023 | New: embedded pwd reset |


## Tips and tricks

### Prevent OTP email send if email invalid

Use DisplayCntrols starter pack and the following override:

```
      <DisplayControl Id="emailVerificationControl2" UserInterfaceControlType="VerificationControl">
        <Actions>
          <Action Id="SendCode">
            <ValidationClaimsExchange>
              <ValidationClaimsExchangeTechnicalProfile TechnicalProfileReferenceId="AAD-UserReadUsingEmailAddress" />                  
            </ValidationClaimsExchange>   
          </Action>
        </Actions>
      </DisplayControl>            
```
