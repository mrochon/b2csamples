# Support for application roles
Azure B2C, unlike Azure AD, does not provide out-of-the-box support for [application roles](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-add-app-roles-in-azure-ad-apps). However, the underlying data structures used for this support are available through Microsoft Graph and
at times through the AAD portal view of B2C.

## Setup
Based on LocalAccounts starter pack. To use it with any other starer pack, please modify the orchestration step numbers in the RoleExtensions.xml.

Setup instructions for the Azure Function are provided in the [/source](https://github.com/mrochon/b2csamples/edit/master/Policies/AppRoles/source) folder.

### Defining roles
B2C does not expose the UI for adding application roles. To define define them use the manifest view of the application (in App Registrations). To the application's appRoles property add an array of objects with the [following data structure](https://docs.microsoft.com/en-us/graph/api/resources/approle?view=graph-rest-1.0#properties):
```javascript
{
  "allowedMemberTypes": ["User"],
  "description": "string",
  "displayName": "string",
  "id": "<create a guid>",
  "isEnabled": true,
  "value": "<value to be returned in user token>"
}
```
## Assigning roles

### Using AAD portal

You can use the [Azure AD portal blade](https://aad.portal.azure.com) to assign users to an application role. Make sure to select the application in the Enterprise Applications view. Users and groups sub-menu is the standard AAD UI for assigning groups/users to roles.

### Using PowerShell
You can create groups, assign
these groups to the application roles using the [Graph SDK PowerShell](https://docs.microsoft.com/en-us/graph/powershell/get-started)
 script below and then just assign users to groups using the Azure portal.

```PowerShell
$scopes = @("Application.Read.All","Group.Read.All","AppRoleAssignment.ReadWrite.All")
$appId = "<application id>"
$groupId = '<group object id>'
$roleId = "<role id>"
Connect-MgGraph -TenantId <your tenant name> -scopes $scopes
$svcPrincipal = Get-MgServiceprincipal -filter "AppId eq '$appId'"
New-MgServicePrincipalAppRoleAssignment -ServicePrincipalId $svcPrincipal.Id -AppRoleId $roleId -PrincipalId $groupId -ResourceId $svcPrincipal.Id 
```

### Using Graph

POST 

```https://graph.microsoft.com/v1.0/users/<user object id>/appRoleAssignments```

```Json
{
    "appRoleId": "<app role guid>",
    "principalId": "<user or group id>",
    "resourceId": "<object id of the SERVICE PRINCIPAL of the application>"
}
```

## Adding sample to your policy set

Use [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies) with the following command

```PowerShell
Add-IefPoliciesSample AppRoles -owner mrochon -repo b2csamples
```

## Run-time support
Using the following IEF policies as sample and the the related [REST API defined here](https://github.com/mrochon/b2csamples/tree/master/REST), will result
in the user's id_token containing an array of roles the user has been assigned to for this application.
