# Multi-tenant B2C setup
## Introduction
The following leads you through the process of setting up an Azure AD B2C directory to provide identity (authentication) support for a multi-tenant SaaS application. A single B2C directory is used to maintain each users membership of an application tenant. Once a user authenticates, using either a local B2C account or a federated social or work account, the token issued to the application contains, among other the user's application tenant id.

The REST functions and B2C user policies deployed as part of this setup are used by a sample ASP.NET MVC application simulating the SaaS application. For an running demo deployment go [here](https://b2cmultitenant.azurewebsites.net).

## Source code
The source code used for this deployment resides in several github repositories to facilitate its deployment to Azure Web App service:
- Demo web app (ASP.NET Core MVC app).
- B2C IEF XML policies 
- REST functions used by both the custom policies and the demo app

The PowerShell script provided in this repo performs most of the setup functions needed to deploy all the components of this application into your own B2C tenant.

Some of the setup steps cannot be currently performed with a PowerShell script. These are listed separately.

## Setup steps

### B2C setup
In order to deploy the [custom policies](https://docs.microsoft.com/en-us/azure/active-directory-b2c/custom-policy-overview) used by this sample, your B2C directory requires some [additional setup](https://docs.microsoft.com/en-us/azure/active-directory-b2c/custom-policy-get-started?tabs=applications). You can follow the above reference or use a [custom web app](https://b2ciefsetup.azurewebsites.net/) to execute these operations.

### B2C application registrations

Currently, B2C does not support programmatic registration of OAuth2 apps. Please register the two applications as follows.

**Note:** use the B2C App Registration blade (not preview).

1. REST API app (use same values as shown below and copy the Application ID to the appsettings.json webApi/appId value)
![App details](assets/apireg1.jpg)
![Scopes](assets/apireg2.jpg)

2. Sample MVC app:
![App details](assets/appreg1.jpg)
**Note:** the reply urls above will need to use the correct url of *your* web deployment. Leave these fields blank for now and replace them after the deployment script completes - it will generate the a partly random name for the url.

![Secrets](assets/appreg3.jpg)
![API access](assets/appreg2.jpg)

Update the appsettings.json with webApp/appId with Application ID (1st image above), webApp/secret with secret created in the 2nd screen.

3. Invitation signing key

Use the Identity Experience Framework/Policy Key blade to create a symetric signing key for creating user invitations. Copy the generated value to appsettings.json InvitationKey attribute value. **Note**: the policies are expecting the key to have this name.

![Invitation key](assets/invitationkey.jpg)

### Scripted deployment
MultitenantSetup.ps1 script will perform the following functions:

1. Register the REST API in the B2C directory so it can make Microsoft Graph calls to the B2C tenant (using OAuth2 Client Credentials application tokens).
2. Create a Resource Group
2. Create a Web App Service Plan
3. Create a Web App Service and deploy the ASP.NET demo app from github
4. Create a Web App Service and deploy the REST function app from github
5. Create a certificate to be used by the IEF custom policies to call the REST functions.
6. Assign the certificate to the web service created in #5 above.

Before executing the script, make sure that both [Az Module](https://docs.microsoft.com/en-us/powershell/azure/install-az-ps?view=azps-3.7.0) and [AzureAD **Preview**](https://docs.microsoft.com/en-us/powershell/azure/active-directory/install-adv2?view=azureadps-2.0) modules are installed.

### Admin consent

The REST API registered above requires admin's consent for two Graph scopes it needs: Group.ReadWrite.All and User.Read.All. After the above script comletes, an admin needs to use the AzureAD portal for the B2C directory to consent to these permissions.

### Upload REST authentication certificate

![Upload REST cert](assets/uploadcert.jpg)

### Custom policy deployment
The IEF custom policies can be deployed using the [Upload-IEFPolicies tool](https://github.com/mrochon/b2cief-upload). Before executing that script update its json configuration file with the url of your REST service deployed in #5 above.

