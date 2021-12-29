# Send B2C Email Confirmation with O365

Policies using DisplayControl to do OTP verification of email address on signup. Calls a custom REST function [part of b2crestapis](https://github.com/mrochon/B2CRestApis)
to use O365 Graph to send the email.

## Setup

You will need to implement a REST API, which uses MS Graph to send email. Here is an [example](https://github.com/mrochon/b2csamples/blob/master/REST/B2CRestApis/Controllers/Email.cs).

To download these policies use [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies) with the following command

```PowerShell
Add-IefPoliciesSample b2cSendOtpWithO365 -owner mrochon -repo b2csamples
connect-
```