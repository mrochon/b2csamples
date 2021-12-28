## Identity for multi-tenant SaaS apps for small organizations

### Description
A [sample React SPA application](https://aka.ms/mtb2c) illustrating use of a single Azure B2C directory for a multi-tenant SaaS applications. SaaS applications need to know not only who the user is, but also which of the many organizations the application services the user belongs to. In the SaaS application, each organization is considered to be a separate tenant. To make it more obvious as to the context for using this term, the following will qualify it *application tenant*. In this sample, a single B2C directory maintains the relationship between individuals and the application tenants (organizations) presenting that information as part of the token it issues to the application.

**Note:** an older version of the [sample application (MVC)](https://b2cmultitenant.azurewebsites.net) is still deployed. IEF policies in this folder are used by it. I am planning to retire it in a couple of months.

### Functionality

Most of the sample's functionality subsists in the custom journeys (policies) and a REST API. A React Single Page App exposes this functionality for browser-based interaction.

1. Signin/signup journey allowing a user to define a new application tenant (organization). The user is recorded as owener/administrator of that tenant.
2. Invitation API and custom journey allowing new users to signup/signin to become members of an existing tenant. Only tenant owners can create the url and token needed to initiate this journey.
2. Sign-in journey administrators or members can use to sign in to the SaaS application. The provided OIDC token will include, in additional to the usual user identifying data, claims identifying the application tenant, the user belongs to. Note that a user may belong to several application tenants.

Some additional functionality demonstrated in the sample includes:
1. Combined signin/up, password reset and profile edit journeys
2. Home realm discovery based on invited user's email, e.g. users with an email whose domain represents a valid AAD tenant are automatically directed to that tenant

Users may create local B2C consumer accounts (using their email) or use gmail or any Azure AD directory to authenticate. A user invited with an email from an existing Azure AD tenant will be redirected to that tenant to authenticate.

### Contents

B2C policies supporting signin/signup with simultanoeus creation of an applicaton tenant

**Note:** this sample is **NOT** about [using AAD multi-tenancy](https://docs.microsoft.com/en-us/azure/dotnet-develop-multitenant-applications) to support an application. AAD multi-tenancy is ideal for medium-to-large enterprises who own and manage their own identity infrastructure. This sample is for small enteprises, usually without their own identity infrastructure. It provides support for an application that needs to group it's users into discrete groups, each representing an *application tenant* - a group of people sharing common data in the application. Azure AD B2C allows create their own logins, possibly use some external identity providers (social or work). Using the code provided in this repo, B2C will maintain association between users and *application tenants* and provide that data to your applications when users sign in.

### Source code

The multi-tenant sample consists of three components, each in a separate repo:

1. [IEF policies](https://github.com/mrochon/b2csamples/tree/master/Policies/MultiTenant/policy) implementing B2C user journeys.

2. [REST functions](https://github.com/mrochon/b2csamples/tree/master/Policies/MultiTenant/source/API) used by the policies and the sample application: create new application tenant, add members, get member's tenant, create invitation url, get list of tenant members.

3. [Sample SPA application](https://github.com/mrochon/b2csamples/tree/master/Policies/MultiTenant/source/UI).

### Setup

#### B2C
Using the IEFPolicies PS module:

```PowerShell
Connect-IefPolicies <yourtenantname> -allowInit
Initialize-IefPolicies  # only needed if your B2C is not yet setup for deploying IEF custom journeys
New-IefPoliciesCert RestClient -validityMonths 24
```

#### SPA

Follow https://jyoo.github.io/deploying-react-spa-in-10-minutes-using-azure


**Ignore the following:**
```PowerShell
npx create-react-app UI
cd UI
npm install react-bootstrap bootstrap@5.1.3
npm i @azure/msal-browser
npm i @azure/msal-react
npm i 
npm install axios
copy /src
```

#### API
1. Register an application in your B2C tenant to manage authorization to the REST API

    a. Expose two API permissions: User.Read and User.Invite. Use *https://yourtenant.onmicrosoft.com/mtrest* as Application ID URI.
    b. Give **application** permissions to Graph API: Group.ReadWrite.All, GroupMember.ReadWrite.All, User.Read.All
    c. Create a secret (store it in the API configuration as *ClientCreds:ClientSecret*)
    d. Create another secret, store it in the API configuration as *Invitation:SigningKey* **and** as *InvitationSigningKey* in your b2C tenant). You can then delete it from the application's secrets.
2. Modify appsettings.json as appropriate to your configuration

    a. details of the certificate created above (B2C step)
    b. details of the application registered above
3. Deploy to a publicly accessible url
4. Provide the deployed application with the public key of the certificate created above and make it required for authentication to the app
5. Exclude */tenant/oauth2* path from certificate requirement
6. Enable CORS for the origin of your SPA app url

#### Custom journeys
```PowerShell
cd <...\B2CSamples\Policies\Multitenant\policy>
# modify the conf.json file to reflect your configuration
Connect-IefPolicies <yourtenantname>
Import-IefPolicies -configurationFilePath <your conf>.json
```


