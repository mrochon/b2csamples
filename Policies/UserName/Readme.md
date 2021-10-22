# UserName with Graph batch migration

## Acknowldgement

Based on recommendations from JasS.

## Purpose

Users migrated to B2C using Graph, batch migration are created without a verified email address.
That attribute is not accessible thorugh Graph. Consequently, the default password reset flows, which require that attribute, cannot be executed. The following policies create an extension attribute for verified mail and use it to drive the password reset flow. Note that all users,
whether created through the Batch POST operation or the signup policy have to use this attribute.
Therefore, you should only use the signup policy defined here (or a variant thereof) or the Graph POST statement to create users.

## Microsoft Graph user create

To create users through Graph using the following http POST (with the appropriate OAuth2 access token): `POST https://graph.microsoft.com/beta/users/`

Body:

```javascript
        {
            "accountEnabled": true,
            "creationType": "LocalAccount",
            "displayName": "Test user 05",
            "givenName": "Test",
            "surname": "User",
            "userType": "Member",
            "extension_0428f3354957491e96bb7ce51b81d46a_verifiedEmailAddress": "sailingrock@live.com",
            "identities": [
                {
                    "signInType": "userName",
                    "issuer": "mrochonb2cprod.onmicrosoft.com",
                    "issuerAssignedId": "user06"
                }
            ],
                   "passwordProfile" : {
               "forceChangePasswordNextSignIn": false,
               "password": "TempPassword01"
            }
        }
```

## Policies

To load these policies to your tenant, you can use the [B2CIEF-Upload PowerShell](https://github.com/mrochon/b2cief-upload) script or manually update them with your tenant name, and IEF- and extensions-related appliction ids. Make sure [your tenant is setup for
IEF use](https://b2ciefsetup.azurewebsites.net/).
