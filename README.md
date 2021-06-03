# My B2C samples

## The B2C Multi-tenant sample code has been split into several repos

**In order to provide scripted deployment of (just) the [B2C multi-tenant demo](https://github.com/mrochon/b2csamples/tree/master/Policies/MultiTenant) I have moved
the VS projects related to this demo (UI and reST APIs) to separate repos. This repo will still contain the related
IEF policies as well as policies for other samples and REST functions related to them.**

## Changes

| Date  | Description  |
|---|---|
| May 5th, 2020  | Provided full [deployment script](https://github.com/mrochon/b2csamples/tree/master/Scripts/MultiTenant) for the multi-tenant sample |
| May 13th, 2020  | Added support for per tenant MFA required option |
| Dec 31st, 2020  | Modified MultiTenant setup PS script to allow choice of Azure subscription |
| April 11, 2021  | Moving all REST VS.NET project to the REST folder (all APIs ecept thos for multi-tenant) |
| April 11, 2021  | Adding sample to support Application Roles |
| April 27, 2021  | Adding sample to prevent employees from creating local accounts (policies and REST API) |

## Samples list

| Name  | Description  |
|---|---|
| [AppRoles](https://github.com/mrochon/b2csamples/tree/master/Policies/AppRoles)  | Support for application roles using standard AAD features |
| [B2CSendOTPWithO365](https://github.com/mrochon/b2csamples/tree/master/Policies/b2cSendOtpWith0365)  | Send email OTP using O365 |
| [CheckEmail](https://github.com/mrochon/b2csamples/tree/master/Policies/CheckEmail)  | Prevents users from signing up or in using emails with specific email domains |
| [EmailOrUserId](https://github.com/mrochon/b2csamples/tree/master/Policies/EmailAndUserId)  | Allow users to signup with both an email and a user id and user either to signin later on |
  [EmailOrPhoneMFA](https://github.com/mrochon/b2csamples/tree/master/Policies/EmailOrPhoneMFA)  | Allows local users to use either their email or phone for 2nd FA |
| [ForceADWhenAvaialble](https://github.com/mrochon/b2csamples/tree/master/Policies/ForceAADwhenAvailable)  | Users who signup with an email address supported by an AAD tenant will be automatically redirected there (rather than defining local password in B2C) |
| [IdTokenSelfHint](https://github.com/mrochon/b2csamples/tree/master/Policies/IdTokenSelfHint)  | Allows long-running native apps to initiate profile edit without needing to re-authenticate user |
| [ForceADWhenAvaialble](https://github.com/mrochon/b2csamples/tree/master/Policies/ForceAADwhenAvailable)  | Users who signup with an email address supported by an AAD tenant will be automatically redirected there (rather than defining local password in B2C) |
| [MultiTenant](https://github.com/mrochon/b2csamples/tree/master/Policies/MultiTenant)  | Supports use of a single B2C tenant to support a muli-tenant SaaS application |
