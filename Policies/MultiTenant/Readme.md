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

2. [API application](https://github.com/mrochon/b2csamples/tree/master/Policies/MultiTenant/source/API) used by the policies and the sample application: create new application tenant, add members, get member's tenant, create invitation url, get list of tenant members.

3. [Sample SPA application](https://github.com/mrochon/b2csamples/tree/master/Policies/MultiTenant/source/UI).

### Setup

#### Invitation signing key

Create a random 40 chars string to be used to sign invitation tokens.

#### Custom journeys

1. Install [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies/). **Note:** make sure you have PowerShell 7.x installed first.
2. Create a new empty directory and open VSCode to edit that folder.
3. Open PowerShell window and make the *policy* folder its working directory (you can use Terminal->New window in VSCode. Make sure that terminal is using PowerShell 7.x: *$host.Version*)
4. Sign in with a Global Admin account but make sure it is not an MSA account - use local or corporate B2B instead:
```PowerShell
Connect-IefPolicies <yourtenantname>
```
4. If you have never used B2C with custom journeys (or you are not sure) execute the following:

```PowerShell
Initialize-IefPolicies  
```

5. Download the MFA Starter pack to the working folder. You may want to delete the Relying Party files (SignUpSignin.xml, etc.) as these journeys do not use the multi-tenancy support.
```PowerShell
New-IefPolicies
# Select 'M'
```PowerShell

5. Save the invitation signing key secret in B2C:

```PowerShell
New-IefPoliciesKey InvitationSigningKey -purpose sig -value "<key value>" -validityInMonths 12
```

5. Add the multi-tenant sample policies:
```PowerShell
Add-IefPoliciesSample MultiTenant -owner mrochon -repo b2csamples
```PowerShell

5. Add at least one external IdP. If you only want to allow local accounts, you will need to modify the journeys to not refernece alternativeSecurityId. The following will add support for signing in with any Azure AD (Work or School account). See [other options](https://github.com/mrochon/IEFPolicies#add-iefpoliciesidp) for adding Goggle, FB, etc. After executing this command, you will need to copy contents of the ./federations sub-folder over your current working folder.
```PowerShell
Add-IefPoliciesIdP AAD -name WORK
```PowerShell

6. Create a certificate for your B2C policies to authenticate to the REST functions and deploy it to the RestClient policy key container in B2C. Its public key needs to be provided to the REST API application (see above).

```PowerShell
New-IefPoliciesCert MTRestClient -validityMonths 24
```PowerShell

6. Modify the <yourtenant>.json file to set a version id for your policies (e.g. V1_) and the url of your API Application deployment.

7. Upload your policies to your B2C tenant
```PowerShell
Import-IefPolicies
```

#### Aplication registrations

Register the following applications in your B2C tenant twice:

1. API application as a client with **application** permissions to Graph API: *Directory.Read, Group.ReadWrite.All*. Create a secret for this app and store its configuration in the *APIClientCreds* property of the appSettings.json (secret in secrets.json).

2. API application as a resource server (API) validating tokens received from the SPA application. Use *Expose an API* to define two new scopes: *Members.Read.All* and *User.Invite*. Use *https://yourtenant.onmicrosoft.com/mtrest* as Application ID URI. Store its configuration in the *B2CBearer* section of the API source.

1. SPA application to represent the React application. Use *API Permissions* to grant this application permission to call the REST API with two scopes defined above. Store client is in the appropriate property of the *Invitation* section of appSettings.json

The first two registrations represent the same deployed code. The registration is split into two to allow it by both the SPA application (which uses delegated user tokens) and the B2C IEF policies (which use a client certificate for authentication) and then use an application token to call Grapg (B2C does not allow Graph to be called with a delagated user token).

#### REST API deployment

1. Update the appSettings.json *Invitation:ReplyUrl* with the url of your SPA application.

2. Update the *B2CBearer:Policy* with the name of your SignIn policy (e.g. B2C_1A_V3_SignIn).

3. Update the *Invitation:Policy* with the name of the policy to which users should be directed to redeem invitations

4. Create a random 40 characters string and store it as Invitation:SigningKey property.

4. Deploy the API application. The public component of the certificate created in the *Custom journeys* step above should be provided to the deployed REST app (*Certificates->Public key certificates*). Public key of that certificate will exists as MTRestApi.cer file in the working directory. If deploying to Azure Web Apps, set WEBSITE_LOAD_CERTIFICATES to '*' or the thumbprint of your certificate. Also, in the General Settings, specify the Cerificate exclusion Path as /tenant/oauth2 (these APIs are not called by the IEF policies but only by the UI application and use OAuth2 tokens, not certificate for authorization). 

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




