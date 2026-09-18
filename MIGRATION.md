# Migrating these samples to Microsoft Entra External ID

This guide maps every custom policy sample in this repo to its Microsoft Entra External ID equivalent, and explains what has to be rebuilt, what becomes simple configuration, and what has no equivalent at all.

*Checked against Microsoft Learn documentation, September 2026. External ID is changing quickly — items marked ⚠ are ones I'd verify in a test tenant before committing to a design.*

## Where things stand

- **Azure AD B2C is no longer sold to new customers** (since May 1, 2025). Existing tenants keep working, and Microsoft has committed to supporting B2C until at least May 2030.
- **Azure AD B2C P2 was discontinued** for all customers on March 15, 2026.
- **Microsoft Entra External ID** (external tenant configuration) is the successor.

2030 sounds distant. It isn't, for a B2C estate of any size: every application must be updated to new endpoints, users and credentials must be moved, and every custom policy journey must be redesigned. Teams that start in the last year will be doing it under pressure.

## The one thing to understand first

**External ID has no custom policies.** There is no IEF, no TrustFramework XML, no user journeys, no orchestration steps, no technical profiles. Microsoft's own guidance is that custom policy logic "must be recreated using custom authentication extensions," and that "one-to-one parity isn't guaranteed."

Everything a custom policy did lands in one of three places:

1. **Configuration.** Much of what needed XML in B2C is a built-in setting in External ID — email OTP and SMS MFA, passkeys, Conditional Access, custom attributes, app roles, token lifetimes, SAML relying-party apps, OIDC and SAML/WS-Fed federation, custom URL domains.
2. **Custom authentication extensions.** REST calls at fixed points in the flow. This is the closest thing to a technical profile, but you only get the events Microsoft provides:

   | Event | Fires | Replaces, roughly |
   |---|---|---|
   | `OnAttributeCollectionStart` | Before the sign-up attribute page renders | Pre-fill and pre-validation technical profiles |
   | `OnAttributeCollectionSubmit` | After the user submits sign-up attributes | Validation technical profiles on self-asserted pages |
   | `OnTokenIssuanceStart` | Just before a token is issued | REST claims providers that enrich the token |
   | `OnOtpSend` | When an email OTP is sent | Custom email providers (SendGrid, O365, etc.) |
   | `OnPasswordSubmit` | During sign-in with a password | JIT password migration |

   Note that the attribute collection events apply only to browser-based flows, not to native authentication. There is no general-purpose "call my API at step 4 and branch on the result."
3. **Application code.** Journeys that existed because B2C could host them — profile edit, change email, approval flows, heavily customized UI — generally move into your application, calling Microsoft Graph. [Native authentication](https://learn.microsoft.com/en-us/entra/external-id/customers/concept-native-authentication) gives you full control over the sign-in UI if the hosted pages aren't enough.

## Choosing a migration approach

Microsoft offers two:

- **Standard migration** — recommended for most tenants. Create a new external tenant, move users (and optionally passwords), re-register and cut over applications.
- **High Scale Compatibility (HSC) mode** — only for very large tenants (roughly 5 million+ directory objects). B2C and External ID run side by side *in the same tenant*, users and credentials stay in place, and you move applications one at a time. It currently has significant gaps: no social identity providers, no passkeys, no age gating, limited Conditional Access, and no admin portal — configuration is done through Microsoft Graph.

Below the HSC threshold, Microsoft is explicit that HSC mode "provides no added benefit." Use the standard approach.

## Users and passwords

Bulk user migration through Microsoft Graph is **always** required. The real decision is whether you need to preserve passwords.

You probably don't if your users sign in with social or enterprise IdPs, you're moving to passwordless (email OTP, passkeys), or you can accept a forced password reset. Otherwise there are two patterns:

- **JIT migration in External ID** — apps move to External ID first. On first sign-in, the `OnPasswordSubmit` extension validates the password against B2C, writes it into the External ID account, and flags the user as migrated. This is the productized successor to the [JitMigrate](Policies/JitMigrate) sample.
- **B2C-initiated harvesting** — apps stay on B2C while a *custom policy* validates each user's password and writes it to External ID in the background; apps cut over once enough users have moved. This one is worth noting: your IEF skills are still useful *during* the migration.

Either way, users who never sign in never migrate, and running both systems in parallel gets harder the longer it lasts. Time-box the coexistence period and plan a final forced reset for stragglers.

Microsoft publishes a [B2C to External ID migration tool](https://github.com/microsoft/b2c-to-meeid-migration-tool) covering user export, import, and the JIT Azure Function.

## Sample-by-sample

**✅ Configuration** — a built-in feature covers it · **🔧 Extension** — rebuild as a custom authentication extension · **🧩 Redesign** — moves into application code or needs a new design · **⛔ Gap** — no equivalent today

| Sample | External ID path | Status |
|---|---|---|
| [AllInOne](Policies/AllInOne) | No profile-edit journey. Build profile edit in your app and write attributes through Microsoft Graph. | 🧩 |
| [AppRoles](Policies/AppRoles) | App roles are supported natively. Group and app role support is still being phased in — check [Microsoft's support matrix](https://learn.microsoft.com/en-us/entra/external-id/customers/reference-group-app-roles-support). | ✅ |
| [B2B2B2C](Policies/B2B2B2C) | External ID can register SAML relying-party apps, but this sample's email-rewriting trick needs claims customization, and the whole pattern should be re-tested. | 🧩 ⚠ |
| [BatchMigration](Policies/BatchMigration) | Bulk-create users through Microsoft Graph or Microsoft's migration tool. Self-service password reset (email OTP or SMS) is built in. | ✅ |
| [CallGraph](Policies/CallGraph) | Call your own API from `OnTokenIssuanceStart` (custom claims provider); the API calls Graph. | 🔧 |
| [ChangeLocalEmail](Policies/ChangeLocalEmail) | No built-in change-email journey. Implement in the app: verify the new address, then update the identity through Graph. | 🧩 |
| [CheckEmail](Policies/CheckEmail) | Block domains at sign-up with `OnAttributeCollectionStart` or `OnAttributeCollectionSubmit` (`showBlockPage`). Blocking at *sign-in* has no equivalent hook — handle it at sign-up, or in the app. | 🔧 |
| [ClaimsEncryption](Policies/ClaimsEncryption) | Return already-encrypted values from your API in `OnTokenIssuanceStart`. Decryption stays in the relying party. | 🔧 |
| [ConditionalAccess](Policies/ConditionalAccess) | Native Conditional Access: block, require MFA, or require password reset, conditioned on device platform and location. Risk-based conditions are **not** available — ID Protection isn't offered in external tenants. | ✅ |
| [ContinueOnOTPVerified](Policies/ContinueOnOTPVerified) | Hosted-page behavior can't be scripted. For this level of UX control, use native authentication. | 🧩 |
| [CustomRefreshTokenExpiryTime](Policies/CustomRefreshTokenExpiryTime) | Use Conditional Access sign-in frequency to force re-authentication, rather than a custom refresh token journey. | ✅ |
| [EmailAndUserId](Policies/EmailAndUserId) | Depends on current username / alias sign-in support in External ID — check before designing. | 🧩 ⚠ |
| [EmailMFA](Policies/EmailMFA) | Email OTP is a built-in MFA method. | ✅ |
| [EmailOrPhoneMFA](Policies/EmailOrPhoneMFA) | Email OTP and SMS are both built-in MFA methods. | ✅ |
| [EmailOTPWithGoogleRecaptchaV3](Policies/EmailOTPWithGoogleRecaptchaV3) | Passwordless email OTP sign-in is built in. Replace reCAPTCHA with the built-in fraud-protection integrations (Arkose Labs, HUMAN Security) or a WAF (Cloudflare, Akamai). | ✅ |
| [EmbeddedPwdReset](Policies/EmbeddedPwdReset) | Self-service password reset is part of the standard sign-in flow. | ✅ |
| [HRD](Policies/HRD) | Configure the enterprise IdPs natively; do the email-domain routing in your app before redirecting. Verify current domain-hint behavior. | 🧩 ⚠ |
| [IdTokenSelfHint](Policies/IdTokenSelfHint) | Profile edit moves into the app (Graph), so the self-hint pattern isn't needed. | 🧩 |
| [invitation](Policies/invitation) | External ID's invitation feature is for administrators only, not customers. Validate an invitation code in `OnAttributeCollectionSubmit` instead — Microsoft cites exactly this use case. | 🔧 |
| [JitMigrate](Policies/JitMigrate) | Now a first-class feature: the `OnPasswordSubmit` extension. See *Users and passwords* above. | 🔧 |
| [MFAChoice](Policies/MFAChoice) | Email OTP and SMS: yes. Authenticator app / TOTP is **not** an External ID MFA method; passkeys are the stronger alternative. | ✅ / ⛔ |
| [MultiTenant](Policies/MultiTenant) | External ID app registrations are always single-tenant. Federate with each customer's Entra ID and derive the organization from `OnTokenIssuanceStart` claims. A real redesign. | 🧩 |
| [PassPromptToAAD](Policies/PassPromptToAAD) | Test how `prompt` is handled with Entra ID federation in your tenant. | 🧩 ⚠ |
| [PersistCustomAttr](Policies/PersistCustomAttr) | Custom user attributes are built in and can be added to sign-up. | ✅ |
| [RefreshToken](Policies/RefreshToken) | Use `revokeSignInSessions` in Microsoft Graph to invalidate a user's refresh tokens and sessions. | ✅ |
| [SendGrid](Policies/SendGrid) | `OnOtpSend` — custom email provider for one-time passcodes. | 🔧 |
| [b2cSendOtpWith0365](Policies/b2cSendOtpWith0365) | `OnOtpSend` — custom email provider for one-time passcodes. | 🔧 |
| [StepUpMFA](Policies/StepUpMFA) | Conditional Access authentication context, in standard deployments (not HSC mode). | ✅ |
| [TOTP](Policies/TOTP) | Authenticator app / TOTP isn't an External ID MFA method. Passkeys (FIDO2) are the nearest supported option. | ⛔ |
| [TOTPBasedApproval](Policies/TOTPBasedApproval) | Move the approval step into the application or API. | 🧩 |
| [UseUserInfoForEmailClaim](Policies/UseUserInfoForEmailClaim) | Entra ID federation is built in. If the email is still missing, fetch it in `OnTokenIssuanceStart`. | 🔧 ⚠ |
| [UserInfo](Policies/UserInfo) | Standard OIDC UserInfo; no policy needed. | ✅ |
| [UserNameWithEmailVerify](Policies/UserNameWithEmailVerify) | Rethink as email-based identities, or combine with the username support noted under *EmailAndUserId*. | 🧩 ⚠ |
| [UI](UI) / [Tools/CustomUI](Tools/CustomUI) | Hosted pages support company branding, not B2C's arbitrary HTML page templates. For full control, use native authentication. | 🧩 |

## Gaps that surprise teams

Beyond the samples, these catch migrations out most often:

- **Custom UI.** B2C let you host your own HTML and JavaScript. External ID's hosted pages are branded, not replaced. If your B2C UI is heavily customized, budget for native authentication.
- **Social IdPs configured in custom policies** don't come across — reconfigure them with External ID's built-in providers. Arbitrary third-party IdPs configured through custom policies aren't supported.
- **Age gating** isn't supported.
- **Customer invitations** aren't supported natively (see *invitation* above).
- **Apps are single-tenant only**, which changes any multi-tenant SaaS design.
- **Resource owner password credentials (ROPC)** isn't supported; use native authentication for mobile.
- **Risk-based Conditional Access and ID Protection** aren't available in external tenants.
- **Application API permissions** are limited to `openid`, `offline_access`, `User.Read`, and your own APIs.
- **Log retention is seven days** unless you export to Azure Monitor. User Insights retires on August 31, 2026.

## Planning checklist

1. **Inventory your policy set.** `Export-IEFPolicies` from [IefPolicies](https://www.powershellgallery.com/packages/IefPolicies/) downloads every policy in the tenant to a local folder, and `Debug-IEFPolicies` flags common errors — worth running on an estate you've inherited. Then list every journey, technical profile, REST call, IdP, and claims transformation.
2. **Classify each item** using the four statuses above. Anything marked 🧩 or ⛔ needs a design decision before a timeline is credible.
3. **Inventory the applications** — including any owned by third parties. The migration isn't complete until every one is re-pointed.
4. **Decide on passwords** — reset, JIT, or B2C-initiated harvesting.
5. **Pick the approach** — standard, unless you're above ~5 million objects and can live with HSC's gaps.
6. **Build in a test tenant first**, starting with the ⚠ items.

## Getting help

I wrote these samples and the IefPolicies module, and I now help teams plan and deliver B2C migrations: estate assessments, custom policy redesign, and rescue work on estates nobody in-house still understands. Engagements run one week to three months. [email] · [LinkedIn]

## Microsoft documentation

- [Plan your migration from Azure AD B2C to External ID](https://learn.microsoft.com/en-us/entra/external-id/customers/plan-your-migration-from-b2c-to-external-id)
- [Migrate from Azure AD B2C to Microsoft Entra External ID](https://learn.microsoft.com/en-us/entra/external-id/customers/migrate-from-b2c-to-external-id)
- [Just-in-time password migration](https://learn.microsoft.com/en-us/entra/external-id/customers/how-to-migrate-passwords-just-in-time)
- [Custom authentication extensions](https://learn.microsoft.com/en-us/entra/external-id/customers/concept-custom-extensions)
- [External tenant features](https://learn.microsoft.com/en-us/entra/external-id/customers/concept-supported-features-customers)
- [Service limits and capability support](https://learn.microsoft.com/en-us/entra/external-id/customers/reference-service-limits)
- [Azure AD B2C FAQ](https://learn.microsoft.com/en-us/azure/active-directory-b2c/faq)
- [B2C to External ID migration tool](https://github.com/microsoft/b2c-to-meeid-migration-tool)
