# Azure AD B2C custom policy samples

Working Azure AD B2C custom policy (IEF) samples, plus the tooling I use to build and deploy them — the [IefPolicies](https://www.powershellgallery.com/packages/IefPolicies/) PowerShell module and the [B2C Extensions](https://marketplace.visualstudio.com/items?itemName=AzureADB2CTools.aadb2c) for VS Code.

> **Moving off B2C?** Azure AD B2C is no longer sold to new customers (since May 1, 2025) and Microsoft's successor is Microsoft Entra External ID. External ID has no custom policies, so every IEF journey has to be rebuilt as configuration, custom authentication extensions, or application code. **[MIGRATION.md](MIGRATION.md)** maps each sample in this repo to its External ID equivalent.

### About me

I'm Marius Rochon, formerly a technical consultant at Microsoft specializing in identity, authentication and secure application development. I now work independently, on:

- **Azure AD B2C → Entra External ID migration** — estate assessment, custom policy migration, and rescue work for B2C estates that nobody in-house still understands
- **Identity for AI agents** — Microsoft Entra Agent ID, agent delegation and authorization, MCP security
- **Training** — private courses on custom policies, OAuth 2.x, and agent identity

Engagements run one week to three months. mrochon@merariai.com · www.linkedin.com/in/marrochon

---

## Samples

| Name | Description |
|---|---|
| [AllInOne](Policies/AllInOne) | Allow profile edit during sign-in or password reset |
| [AppRoles](Policies/AppRoles) | Application roles using standard Entra ID app role features |
| [B2B2B2C](Policies/B2B2B2C) | Use B2C as a SAML IdP for Entra ID direct federation (supersedes *SAMLIdP*) |
| [BatchMigration](Policies/BatchMigration) | Batch user creation, with an email inviting users to set their password via `login_hint` |
| [CallGraph](Policies/CallGraph) | Call a Microsoft Graph API from a journey |
| [ChangeLocalEmail](Policies/ChangeLocalEmail) | Change the sign-in email address of an existing local account |
| [CheckEmail](Policies/CheckEmail) | Prevent sign-up or sign-in with emails from specific domains |
| [ClaimsEncryption](Policies/ClaimsEncryption) | Encrypt and decrypt claims in a token |
| [ConditionalAccess](Policies/ConditionalAccess) | Conditional Access rules to block, require MFA, or require a password change |
| [ContinueOnOTPVerified](Policies/ContinueOnOTPVerified) | Proceed to the new-password screen as soon as the OTP is verified, without a Continue click |
| [CustomRefreshTokenExpiryTime](Policies/CustomRefreshTokenExpiryTime) | Refresh token lifetime shorter than B2C's 24-hour minimum, enforced by a REST function |
| [EmailAndUserId](Policies/EmailAndUserId) | Sign up with both an email and a user id; sign in later with either |
| [EmailMFA](Policies/EmailMFA) | Email OTP as a second factor, for local and federated accounts |
| [EmailOrPhoneMFA](Policies/EmailOrPhoneMFA) | Local users choose email or phone as their second factor |
| [EmailOTPWithGoogleRecaptchaV3](Policies/EmailOTPWithGoogleRecaptchaV3) | Passwordless sign-in with an email OTP, protected by Google reCAPTCHA v3 |
| [EmbeddedPwdReset](Policies/EmbeddedPwdReset) | Password reset as a user-selectable option inside the sign-in journey |
| [HRD](Policies/HRD) | Home realm discovery: redirect to the configured IdP for the user's email domain |
| [IdTokenSelfHint](Policies/IdTokenSelfHint) | Let long-running native apps start profile edit without re-authenticating the user |
| [invitation](Policies/invitation) | Invitation links using a `client_assertion` JWT, for local-only or federated-only accounts |
| [JitMigrate](Policies/JitMigrate) | Just-in-time migration: verify legacy passwords through an API at first sign-in |
| [MFAChoice](Policies/MFAChoice) | User chooses email OTP, phone OTP/SMS/call, or Authenticator TOTP as second factor |
| [MultiTenant](Policies/MultiTenant) | One B2C tenant serving a multi-tenant SaaS application |
| [PassPromptToAAD](Policies/PassPromptToAAD) | Pass the OIDC `prompt` parameter through to a federated Entra ID |
| [PersistCustomAttr](Policies/PersistCustomAttr) | Add a new persisted custom user attribute to a starter pack |
| [RefreshToken](Policies/RefreshToken) | Reject refresh token redemption after the user has requested revocation |
| [SendGrid](Policies/SendGrid) | Send verification and OTP emails through SendGrid |
| [b2cSendOtpWith0365](Policies/b2cSendOtpWith0365) | Send email OTPs through Office 365 |
| [StepUpMFA](Policies/StepUpMFA) | Require MFA even if the user completed it recently |
| [TOTP](Policies/TOTP) | Authenticator app (TOTP) as a second factor |
| [TOTPBasedApproval](Policies/TOTPBasedApproval) | Use a third-party TOTP value to approve an application action |
| [UseUserInfoForEmailClaim](Policies/UseUserInfoForEmailClaim) | Get the user's email from the Entra ID UserInfo endpoint when the id_token doesn't include it |
| [UserInfo](Policies/UserInfo) | Demonstrates the B2C UserInfo endpoint |
| [UserNameWithEmailVerify](Policies/UserNameWithEmailVerify) | Username accounts created by Graph batch migration, with a verified-email attribute driving password reset |

Most samples can be added to an existing policy set with IefPolicies:

```PowerShell
Add-IefPoliciesSample <SampleName> -owner mrochon -repo b2csamples
```

Supporting code: [REST](REST) (Azure Functions called from policies), [UI](UI) (custom page templates), [Tools/CustomUI](Tools/CustomUI), [Scripts](Scripts), and [apps/B2CUsingGraph](apps/B2CUsingGraph).

## Dev environment setup

### Software tools
- [Visual Studio Code](https://code.visualstudio.com/Download). **Recommendation:** set *File → Auto Save* to *On*
- [B2C Extensions](https://marketplace.visualstudio.com/items?itemName=AzureADB2CTools.aadb2c)
- An XML extension, e.g. [Red Hat XML](https://marketplace.visualstudio.com/items?itemName=redhat.vscode-xml)
- (Optional) add this XML file association to your VS Code `settings.json`:
```json
    "xml.fileAssociations": [
        {
          "pattern": "**.xml",
          "systemId": "https://raw.githubusercontent.com/Azure-Samples/active-directory-b2c-custom-policy-starterpack/master/TrustFrameworkPolicy_0.3.0.0.xsd"
        }
      ],
```
- [PowerShell 7.x](https://learn.microsoft.com/en-us/shows/it-ops-talk/how-to-install-powershell-7)
- [IefPolicies](https://www.powershellgallery.com/packages/IefPolicies/) ([documentation](https://github.com/mrochon/IEFPolicies))
- [SAML2 test service provider](https://samltestapp2.azurewebsites.net/). Register an app in your B2C tenant with this app's issuer id and reply URL

### GitHub Codespaces
If you have access to GitHub Codespaces, copy the [.devcontainer](.devcontainer) folder into the root of your project. The codespace is configured as described above.

### Dev B2C tenant setup
- Register a web app (Token Viewer) with reply URL *https://oidcdebugger.com/debug*, and allow return of access and id tokens on the Authentication tab
- Create a user (*Users → New user → Create user*) with the B2C UPN, e.g. *someuser@myb2c.onmicrosoft.com* — useful for experimenting with Microsoft Graph through [Graph Explorer](https://aka.ms/ge)
- Use *Invite user* to add the corporate users who will manage B2C development, and give them the admin roles they need
- Use the [B2C Setup tool](https://aka.ms/b2csetup) to initialize B2C for IEF use

### Usage example

1. Open VS Code
2. Select *Terminal → New Terminal*
3. Make sure the terminal is running PowerShell 7.x (`$host.Version`)
4. Create a new folder and change to it (e.g. `mkdir myProject; cd myProject`)
5. Run `New-IefPolicies`
6. Select a starter pack, e.g. *SL* (social and local accounts)
7. Run `Connect-IefPolicies <your B2C name>` (the *onmicrosoft.com* suffix isn't needed)
8. Follow the displayed instructions to sign in
9. Run `Import-IefPolicies`
10. The starter pack is modified for your tenant and uploaded (the modified files are in the `./debug` folder)
11. Run your policies from the B2C menus at https://portal.azure.com
12. Re-running `Import-IefPolicies` uploads the policies modified since the last import, plus any policies that depend on them — e.g. modifying `TrustFrameworkBase.xml` re-imports every policy, since they all build on it

## Tips and tricks

### Prevent OTP email send if email invalid

Use the DisplayControls starter pack and the following override:

```xml
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

## Changes

| Date | Change |
|---|---|
| Sep 2026 | New: [MIGRATION.md](MIGRATION.md) — mapping every sample to Microsoft Entra External ID. Fixed broken sample links; added missing samples to the list |
| Jan 2025 | New: TOTP as approval code |
| Mar 2023 | New: call Graph |
| Nov 2022 | New: user choice of second factor |
| Jun 2022 | New: batch migration |
| Mar 2022 | New: step-up MFA |
| Mar 2022 | Fixed: refresh token |
| Feb 2022 | New: claims encryption |
| Feb 2022 | New: use the Entra ID UserInfo endpoint to get the user's email address (when the id_token doesn't include it) |
| Dec 2021 | Change: multi-tenant sample now uses a new SPA app and updated policies and REST functions |
| Dec 2021 | New: optionally allow profile edit during sign-in |
| Nov 2021 | New: persisted custom attribute |
| Oct 2021 | New: Conditional Access |
| Oct 2021 | Change: invitation sample supports local-only or federated-only accounts |
| Oct 2021 | Change: added PowerShell script to assign a group to an app role (AppRoles sample) |
| Sep 2021 | Change: simplified invitation sample |
| Sep 2021 | New: JIT migration |
| Sep 2021 | New: federate B2C as IdP for Entra ID (direct federation) |
