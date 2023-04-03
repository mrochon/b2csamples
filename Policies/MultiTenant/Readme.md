## Identity for multi-tenant SaaS apps for small organizations

### Description
A [sample React SPA application](https://aka.ms/mtb2c) illustrating use of a **single Azure B2C directory** for a **multi-tenant SaaS application**. SaaS applications typically support many organizations. In terms of user identity, they need to know not only who the user is, but also which organization the user belongs to. In the SaaS application, each organization is considered to be a separate tenant. To make it more obvious as to the context for using this term, the following will qualify it as an *application tenant*. In this sample, a single B2C directory maintains the relationship between individuals and the application tenants (organizations) presenting that information as part of the token it issues to the application. In this implementation, the same user may belong to multiple tenants. The user will need to choose at signin time, on behalf of which tenant they are signing in.

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

The multi-tenant sample consists of three components, each in a separate folder:

1. [Custom B2C journeys, Identity Experience Framework xml policy set](https://github.com/mrochon/b2csamples/tree/master/Policies/MultiTenant/policy).

2. [API application](https://github.com/mrochon/b2csamples/tree/master/Policies/MultiTenant/source/API) used by both the policies and the sample application to create new application tenants and maintain user associations with them.

3. [Sample SPA React application](https://github.com/mrochon/b2csamples/tree/master/Policies/MultiTenant/source/UI).

### Setup

#### Invitation signing key

Create a random 40 chars string to be used to sign invitation tokens. It will be used to sign invitation tokens issued by application tenant owners to users invited to join particular tenants.

#### Custom journeys

1. Install [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies/). **Note:** make sure you have PowerShell 7.x installed first.
2. Create a new empty directory and open VSCode to edit that folder.
3. Open PowerShell window and make the *policy* folder its working directory (you can use Terminal->New window in VSCode. Make sure that terminal is using PowerShell 7.x: *$host.Version*)
4. Connect to B2C with a Global Admin account (must not be a MSA account - use local or corporate B2B instead):
```PowerShell
Connect-IefPolicies <yourtenantname; onmicrosoft.com is not needed>
```
5. If you have never used this B2C with custom journeys (or you are not sure) execute the following to initialize support for using IEF:

```PowerShell
Initialize-IefPolicies  
```

6. Download the B2C MFA Starter pack to the working folder. You can delete the Relying Party files (SignUpSignin.xml, ProfileEdit.xml and PasswordReset.xml) as these journeys do not provide for the multi-tenancy support.
```PowerShell
New-IefPolicies
# Select 'M'
```

7. Save the invitation signing key secret in B2C:

```PowerShell
New-IefPoliciesKey InvitationSigningKey -purpose sig -value "<key value>" -validityInMonths 12
```

8. Add the multi-tenant sample policies:
```PowerShell
Add-IefPoliciesSample MultiTenant -owner mrochon -repo b2csamples
```

9. Add at least one external IdP. If you only want to allow local accounts, you will need to modify the journeys to not refernece alternativeSecurityId. The following will add support for signing in with any Azure AD (Work or School account). See [other options](https://github.com/mrochon/IEFPolicies#add-iefpoliciesidp) for adding Goggle, FB, etc. After executing this command, you will need to copy contents of the ./federations sub-folder over your current working folder.
```PowerShell
Add-IefPoliciesIdP AAD -name WORK
```PowerShell

10. Create a certificate for your B2C policies to authenticate to the REST functions and deploy it to the RestClient policy key container in B2C. Its public key needs to be provided to the REST API application (see above).

```PowerShell
New-IefPoliciesCert MTRestClient -validityMonths 24
```

11. Modify the <yourtenant>.json file to set a version id for your policies (e.g. V1_) and the url of your API Application deployment.

12. Upload your policies to your B2C tenant
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

5. Deploy the API application. The public component of the certificate created in the *Custom journeys* step above should be provided to the deployed REST app (*Certificates->Public key certificates*). Public key of that certificate will exists as MTRestApi.cer file in the working directory. If deploying to Azure Web Apps, set WEBSITE_LOAD_CERTIFICATES to '*' or the thumbprint of your certificate. Also, in the General Settings, specify the Cerificate exclusion Path as /tenant/oauth2 (these APIs are not called by the IEF policies but only by the UI application and use OAuth2 tokens, not certificate for authorization). 

#### SPA

1. Update ./UI/src/authConfig.js file for your deployment

2. Build the app

```
npm run build
```
3. Deploy the ./build directory to a web server, e.g. Azure Storage Static web (just copy the ./build folder to the $web container)

Also, see https://jyoo.github.io/deploying-react-spa-in-10-minutes-using-azure
or

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




