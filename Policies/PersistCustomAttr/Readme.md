# New, persisted custom attribute
Adds a new custom user claim/attribute and modifies relevant TechnicalProfiles to write/read it to/from B2C storage.

## Setup
Can be used with any starter pack. 

You can use [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies) with the following command to add the AttrExtensions.xml policy to your current policy set:

```PowerShell
Add-IefPoliciesSample PersistCustomAttr -owner mrochon -repo b2csamples
```
AttrExtensions.xml defines a new claim (*extension_LoyaltyId*) and modifies all relevant TechnicalProfiles to handle it (read/write/display).

To use it, place it in the appropriate position in your policcy xml structure. As provided, it is based on the LocalizationExtensions. Therefore you will need to modify the Extensions.xml to be based on this file, with the following xml in TrustFrameworkExtensions.xml:

```xml
  <BasePolicy>
    <TenantId>yourtenant.onmicrosoft.com</TenantId>
    <PolicyId>B2C_1A_AttrExtensions</PolicyId>
  </BasePolicy>
```

Do a global replace to modify the name of the claim (currently *extension_LoyaltyId*) to whatever name you want to use (preserve the *extension_* prefix). 

You can add new attributes using similar definition. You will then need to add appropriate *Input-/Output-/Persisted-Claim* wherever there is currently a reference to the *extension_LoyaltyId*



