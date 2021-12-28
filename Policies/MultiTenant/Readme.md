## Identity for multi-tenant SaaS apps for small organizations

### Description
A [sample application](https://b2cmultitenant.azurewebsites.net) illustrating use of a single Azure B2C directory for a multi-tenant SaaS applications. A SaaS application need to know not only who the user is but also which of the many organizations the application services the user belongs to. In the SaaS application, each organization is considered to be a separate application tenant. In this sample, a single B2C directory maintains the relationship between individuals and the application tenants (organizations) presenting that information as part of the token it issues to the application.

### Functionality

Most of the sample's functionality subsists in the custom journeys (policies) and a REST API. A React Single Page App exposes this functionality for browser-based interaction.

1. Signin/signup journey allowing a user to define a new application tenant (organization). The user is recorded as owener/administrator of that tenant.
2. Invitation API and custom journey allowing new users to signup/signin to become members of an existing tenant. Only tenant owners can create the url and token needed to initiate this journey.
2. Sign-in journey administrators or members can use to sign in to the SaaS application. The provided OIDC token will include, in additional to the usual user identifying data, claims identifying the application tenant, the user belongs to. Note that a user may belong to several application tenants.

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

#### SPA
npx create-react-app UI
cd UI
npm install react-bootstrap bootstrap@5.1.3
npm i @azure/msal-browser
npm i @azure/msal-react
npm i 
npm install axios
copy /src

#### API

#### Custom journeys


