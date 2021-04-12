# Support for application roles
Azure B2C, unlike Azure AD, does not provide out-of-the-box support for [application roles](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-add-app-roles-in-azure-ad-apps). However, the underlying data structures used for this support are available through Microsoft Graph and
at times through the AAD portal view of B2C.

## Setup
### Defining roles
B2C does not expose the UI for adding application roles. To define define them use the manifest view of the application (in App Registrations). To the application's appRooles property add an array of objects with the [following data structure](https://docs.microsoft.com/en-us/graph/api/resources/approle?view=graph-rest-1.0#properties):
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
You can open the AAD portal view;s Enterprise Tab to assign users to the defined roles

## Run-time support
Using the following IEF policies as sample and the the related [REST API defined here](https://github.com/mrochon/b2csamples/tree/master/REST), will result
in the user's id_token containing an array of roles the user has been assigned to for this application.