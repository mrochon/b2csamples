# Send B2C Email Confirmation with O365

Policies using DisplayControl to do OTP verification of email address on signup. Uses an API to
call Microsoft Graph to send the email.

## Setup

1. Register a new application in the AzureAD, which controls access to your O365 mailboxes. Give it Application permission to Send.Mail (from any user). You can [restrict the access to
a selected mailbox sending the OTP emails](https://docs.microsoft.com/en-us/graph/auth-limit-mailbox-access).
2. Deploy [the REST function](https://github.com/mrochon/b2csamples/tree/master/Policies/b2cSendOtpWith0365/source) to Azure Functions. Configure it to operate as the above regsitered application.
3. Add the Azure Function access key as B2C PolicyKey named B2C_1A_AzureFuncKey (set its purpose property to *encryption*).
4. Add the O365Extensions policy to your policy set. It is currently configured as based of the ExtensionLocalization policy so change your extension poliy(ies) to be based of the O365Extensions (that way you do not need to re-base all your RelyingParty policies).

If using the [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies) you can add this extensions to your existing set using the following PS command:

```PowerShell
cd /mypolicyfolder
Add-IefPoliciesSample b2cSendOtpWith0365 -owner mrochon -repo b2csamples
```