# Using B2C id_token_hint to initiate the edit journey

## Based on

Local starter pack. To use with Social starter packs, ProfileEdit2.xml needs modification. IdTokenHintExtensions contains TPs usable in other RelyingParties.

## Purpose
For native apps, allows silent (no user interaction) token requests using journeys other than the one used earlier to obtain the initial tokens (and refresh tokens). For example, an app may have used a SignIn/Up journey initially and some days later wants to start the Profile edit journey. Since by then any browser cookies would have expired, that request will challenge the user for a new signin (you cannot re-use the original refresh tokens as in B2C refresh tokens are for specific endpoints). This work-around shows to use the original refresh token to re-initialize browser cookies by calling the original endpoint and then use the new id_token as id_token_hint to make the profile edit journey silent.

## Adding sample to your policy set

Use [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies) with the following command

```PowerShell
Add-IefPoliciesSample IdTokenSelfHint -owner mrochon -repo b2csamples
```