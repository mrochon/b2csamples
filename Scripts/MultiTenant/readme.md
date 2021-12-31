# Multi-tenant B2C setup

**Note:** I am replacing the original source code for this sample with a combination of [React SPA and REST MVC app](https://github.com/mrochon/b2csamples/tree/master/Policies/MultiTenant). Some custom journeys are modified as well. The underlying B2C data remains the same except for a change to the federation technical profile for AAD federation. As a consequence, any existing AAD federated users will need to re-sign up (and be invited). This repo will be deleted in a couple of months.

## Introduction
The following leads you through the process of setting up an Azure AD B2C directory to provide identity (authentication) support for a multi-tenant SaaS application. A single B2C directory is used to maintain each users membership of an application tenant. Once a user authenticates, using either a local B2C account or a federated social or work account, the token issued to the application contains, among other the user's application tenant id.

The REST functions and B2C user policies deployed as part of this setup are used by a sample ASP.NET MVC application simulating the SaaS application. For an running demo deployment go [here](https://b2cmultitenant.azurewebsites.net).

## Source code
The source code used for this deployment resides in several github repositories to facilitate its deployment to Azure Web App service:
- [Demo web app](https://github.com/mrochon/b2c-mt-webapp) (ASP.NET Core MVC app).
- [B2C IEF XML policies](https://github.com/mrochon/b2csamples/tree/master/Policies/MultiTenant) 
- [REST functions](https://github.com/mrochon/b2c-mt-rest) used by both the custom policies and the demo app

The PowerShell scripts provided in this folder deploy or remove the WebApp, REST functions and configuration data needed to support custom policy deployment.

## Deployment summary

| Step  | Purpose  | Tool  |
|---|---|---|
| 1  | Create a B2C tenant and [set it up for IEF custom policy](https://docs.microsoft.com/en-us/azure/active-directory-b2c/custom-policy-get-started?tabs=applications) use. This is once only task. |  [B2C IEF setup](https://b2ciefsetup.azurewebsites.net/) |
| 2  | Create all required B2C and Azure web artifacts.  |  [Setup-B2CMultiTenant.ps1](https://github.com/mrochon/b2csamples/blob/master/Scripts/MultiTenant/Setup-B2CMultiTenant.ps1) |
| 3  | Provide admin consent to the REST API to call Graph  | Manual  |
| 4 | Modify and upload custom IEF policies (optional) |  [IEF upload tool](https://github.com/mrochon/b2cief-upload) |

You can use the Remove-B2CMultiTenant.ps1 in this folder to remove items created in step 2.


## Detailed instructions

### B2C setup
In order to deploy any [IEF custom policies](https://docs.microsoft.com/en-us/azure/active-directory-b2c/custom-policy-overview), your B2C directory requires some [additional setup](https://docs.microsoft.com/en-us/azure/active-directory-b2c/custom-policy-get-started?tabs=applications). You can either follow the above reference or use a [custom web app](https://b2ciefsetup.azurewebsites.net/) to perform the setup.


### Install required PowerShell modules
- Install-Module -Name MSAL.PS -RequiredVersion 2.5.0.1
- Install-Module -Name AzureADPreview
- Install-Module -Name Az

### Create deployment data
- Copy this folder to a temporary local folder.
- Create an app registration in your B2C tenant to enable the PowerShell scripts to execute Graph REST API on behalf of the signed-in user. Use the regular AAD blade App Registration (not the B2C blade):
    1. Name: B2CSetup (or whatever you choose)
    2. Public client
    3. Reply url: *urn:ietf:wg:oauth:2.0:oob*
    4. API permissions (all Microsoft Graph user delegated): *openid, offline_access, Directory.AccessAsUser.All*
- Update the setupSettings.json script.clientId property with the application id from the step above
- Update other properties in the setupSettings.json file:
    1. *b2cTenant*: your B2C tenant name 
	2. *subscription*: Azure subscription id (guid) where the web apps (UI and REST) should be deployed, e.g.: "023bba71-19b2-4f09-891f-432e5d50db5a",
    2. *policyPrefix*: a short string to be used in your policy name to disambiguate them from any other policies you may use
    3. *resourceGroup*: name of the Azure resource group to create for this deployment
    4. *location*: id of the Azure datacenter location to be used for deployment
    5. *appServicePlan*: Azure web service plan name. Both the demo app and the REST app will be deployed here
    6. *webApp.name*: name to be used for deploying the demo app. It will be used for app registration in the B2C tenant as well as to create the url of the Azure web app. Make sure it is unique enough for both purposes.
    7. *webApi.name*: same for the REST API app
    9. Leave other items unchanged

### Execute Setup-B2CMultiTenant
- Make sure that your PowerShell console is pointing at the temporary folder with the script and the *setupSettings.json* files
- Run the Setup-B2CMultiTenant.ps1. You will be challenged to login to authorize the script to use Microsoft Graph (directly), AzureAD PowerShell cmdlets and Az cmdlets. Depending on your workstation SSO settings you may be able to login by just providing your AAD upn (email address, no password).
- The script should create RestClientCert.pfx and update the conf.json file. (Neither is no longer needed. I will remove them in the near future). 
- When completed, the script will open a browser page asking you to sign in and grant consent to the REST app to call Microsoft Graph to create security groups (used to represent tenants). Sign in, grant consent. You can ignore the subsequent error page about lacking reply url (REST app has no UI).

### Upload custom policies

The above script will upload the current [IEF multitenant policies](https://github.com/mrochon/b2csamples/tree/master/Policies/MultiTenant), after updating them with your setup and tenant data. If you need to modify them further for some reason (e.g. change or add a user journey, add MFA or another IdP), use the [Upload-IEFPolicies tool](https://github.com/mrochon/b2cief-upload) to upload them. Use the conf.json file updated by the Setup-B2CMultiTenant script to provide the required values. **Note: the Upload-IEFPolicies repo includes some sample policies. Do not upload these. Rather use the policies contained in [this repo](https://github.com/mrochon/b2csamples/tree/master/Policies/MultiTenant).**

