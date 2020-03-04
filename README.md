# b2csamples

## Using B2C to support multi-tenant SaaS apps
### Sample
A [sample application](https://b2cmultitenant.azurewebsites.net) illustrating support for multi-tenant SaaS applications using a single B2C Azure AD tenant. All source is in this repo.

**Note:** this sample is **NOT** about [using AAD multi-tenancy](https://docs.microsoft.com/en-us/azure/dotnet-develop-multitenant-applications) to support an application. AAD multi-tenancy is ideal for medium-to-large enterprises who own and manage their own identity infrastructure. This sample is for small enteprises, usually without their own identity infrastructure. It provides support for an application that needs to group it's users into discrete groups, each representing an *application tenant* - a group of people sharing common data in the application. Azure AD B2C allows create their own logins, possibly use some external identity providers (social or work). Using the code provided in this repo, B2C will maintain association between users and *application tenants* and provide that data to your applications when users sign in.

### Source
[IEF policies](https://github.com/mrochon/b2csamples/tree/master/Policies/MultiTenant) implementing the two signin/up journeys:
- Signin/up AND create a new application tenant
- Signin/up AND join or continue working with an existing application tenant
- Redeem invitation to join a tenant

[REST functions](https://github.com/mrochon/b2csamples/tree/master/REST) used by the policies: create new application tenant, add members, get member's tenant, create invitation url.

[Sample application](https://github.com/mrochon/b2csamples/tree/master/Policies/MultiTenant) source code.

### Setup
To set this multi-tenant SaaS application in your own B2C tenant, you will need to:
1. Clone this source to your own directory
2. Install the [REST functions](https://github.com/mrochon/b2csamples/tree/master/REST) in your own server. See here for [documentation](https://docs.microsoft.com/en-us/azure/app-service/app-service-web-get-started-dotnet-framework).
2. Ensure that your [tenant is setup to use IEF policies](https://docs.microsoft.com/en-us/azure/active-directory-b2c/custom-policy-get-started) or use my [helper application](https://b2ciefsetup.azurewebsites.net/) to do it for you.
3. Modify the [IEF policies](https://github.com/mrochon/b2csamples/tree/master/Policies/MultiTenant) to use your tenant and your REST functions. You can use my (IEF uploader)[https://github.com/mrochon/b2cief-upload] to do it for you. All you need to do is to modify the settings.json file in the Policies folder with your tenant- and app service-specific values.
4. Deploy the sample multi-tenant app to an Azure service (as above for REST). Update it's configuration to use your B2C tenant. (Here is an example)[https://github.com/Azure-Samples/active-directory-b2c-dotnetcore-webapp] of how to regsiter the app.

