# Email and user id journey
B2C custom journeys allowing a local user to use an email address and a user id to sign in. Useful for applications which do not want to disclose users emails to others 
accessing the app, yet identify user specific data by a unique identifier created by the user. Chat or blogging sites are examples of these applications.

Using LocalAccounts starter pack as base.

## Adding sample to your policy set

Use [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies) with the following command

```PowerShell
Add-IefPoliciesSample EmailAndUserId -owner mrochon -repo b2csamples
```