## Identity for multi-tenant SaaS apps for small organizations

### Description
A [sample React SPA application](https://aka.ms/mtb2c) illustrating use of a single Azure B2C directory for a multi-tenant SaaS applications. SaaS applications need to know not only who the user is, but also which of the many organizations the application services the user belongs to. In the SaaS application, each organization is considered to be a separate tenant. To make it more obvious as to the context for using this term, the following will qualify it *application tenant*. In this sample, a single B2C directory maintains the relationship between individuals and the application tenants (organizations) presenting that information as part of the token it issues to the application.

**Note:** an older version of the [sample application (MVC)](https://b2cmultitenant.azurewebsites.net) is still deployed. IEF policies in this folder are used by it. I am planning to retire it in a couple of months. For the time being the custom journey xml files are in the [Scripts folder](https://github.com/mrochon/b2csamples/tree/master/Scripts/MultiTenant).

### Functionality

Most of the sample's functionality subsists in the custom journeys (policies) and a REST API. A React Single Page App exposes this functionality for browser-based interaction.

1. Signin/signup journey allowing a user to define a new application tenant (organization). The user is recorded as owener/administrator of that tenant.
2. Invitation API and custom journey allowing new users to signup/signin to become members of an existing tenant. Only tenant owners can create the url and token needed to initiate this journey.
2. Sign-in journey administrators or members can use to sign in to the SaaS application. The provided OIDC token will include, in additional to the usual user identifying data, claims identifying the application tenant, the user belongs to. Note that a user may belong to several application tenants.

Some additional functionality demonstrated in the sample includes:
1. single journey for signin/up, password reset and profile edit
2. Home realm discovery based on invited user's email, e.g. users with an email whose domain represents a valid AAD tenant are automatically directed to that tenant

Users may create local B2C consumer accounts (using their email) or use gmail or any Azure AD directory to authenticate. A user invited with an email from an existing Azure AD tenant will be redirected to that tenant to authenticate.

**Note:** this sample is **NOT** about [using AAD multi-tenancy](https://docs.microsoft.com/en-us/azure/dotnet-develop-multitenant-applications) to support an application. AAD multi-tenancy is ideal for medium-to-large enterprises who own and manage their own identity infrastructure. This sample is for small enteprises, usually without their own identity infrastructure. It provides support for an application that needs to group it's users into discrete groups, each representing an *application tenant* - a group of people sharing common data in the application. Azure AD B2C allows create their own logins, possibly use some external identity providers (social or work). Using the code provided in this repo, B2C will maintain association between users and *application tenants* and provide that data to your applications when users sign in.

### Source code

The multi-tenant sample consists of three components, each in a separate repo:

1. [IEF policies](https://github.com/mrochon/b2csamples/tree/master/Policies/MultiTenant/policy) implementing B2C user journeys.

2. [REST functions](https://github.com/mrochon/b2csamples/tree/master/Policies/MultiTenant/source/API) used by the policies and the sample application: create new application tenant, add members, get member's tenant, create invitation url, get list of tenant members.

3. [Sample SPA application](https://github.com/mrochon/b2csamples/tree/master/Policies/MultiTenant/source/UI).

### Setup

#### B2C

1. Install [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies/). **Note:** make sure you have PowerShell 7.x installed first.
2. Clone this sample to a local directory, e.g. /mt. 
3. Open a PowerShell window
4. If you have never used B2C with IEF policies (or you are not sure) execute the following:

```PowerShell
Connect-IefPolicies <yourtenantname>
Initialize-IefPolicies  
```

5. Otherwise:

```PowerShell
Connect-IefPolicies <yourtenantname>
```

6. Create a certificate for your B2C policies to authenticate to the REST functions and deploy it to the RestClient policy key container in B2C:

```PowerShell
New-IefPoliciesCert RestClient -validityMonths 24
```

7. Register the REST function app in your B2C tenant as two application: a regular B2C API and an AAD client credentials app
8. The former should expose 2 API permissions: User.Invite and Members.Read.All. Its configuration (id, etc.) should be stored in the *B2C* property of the appSettings.json
9. The latter (Client Creds app) must have MS Graph Application permissions: Directory.Read.All and Group.Readwrite.All.
10. Its app id, tenant id and secret should be deployed as part of the *ClientCreds* property of the REST function appSettings.json 

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
1. Register two applications in your B2C tenant as described above:

    a. In the first registration (through the B2C blade), expose two API permissions: Members.Read.All and User.Invite. Use *https://yourtenant.onmicrosoft.com/mtrest* as Application ID URI.

    b. In the second registration (through the AAD blade), give **application** permissions to Graph API: Directory.Read, Group.ReadWrite.All. Create a secret for this app and store its configuration in the *ClientCreds* property of the appSettings.json (store the secret in the API configuration as *ClientCreds:ClientSecret*)

    d. Create another secret, store it in the API configuration as *Invitation:SigningKey* **and** as *InvitationSigningKey* in your b2C tenant). 

2. The public component of the certificate created in the B2C setup step above should be provided to the deployed REST app. If deploying to Azure Web Apps, set WEBSITE_LOAD_CERTIFICATES to '*'. Also, in the General Settings, specify the Cerificate exclusion Path as /tenant/oauth2 (these APIs are not called by the IEF policies but only by the UI application). 

6. Enable CORS for the origin of your SPA app url

#### Custom journeys
```PowerShell
cd <...\B2CSamples\Policies\Multitenant\policy>
# modify the conf.json file to reflect your configuration
Connect-IefPolicies <yourtenantname>
Import-IefPolicies -configurationFilePath <your conf>.json
```


