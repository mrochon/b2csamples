# JIT User Migration using legacy authentication API

Based on SocialAndLocal starter pack.

This policy implements the [seamless migration strategy](https://docs.microsoft.com/en-us/azure/active-directory-b2c/user-migration#seamless-migration), except that as implemented
here, users do not need to be batch migrated first. The logic of this policy is as follows:
1. collect username/pwd
2. if a user with this name already exists, use regular b2C authentication
3. if not, call an API passing the user's id and password. If that succeeds, create user with this name and password in B2C

## Adding sample to your policy set

Use [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies) with the following command

```PowerShell
Add-IefPoliciesSample JitMigrate -owner mrochon -repo b2csamples
```

A sample REST API for this sample is deployed at the url in .json file of this sample. It returns OK if the first part of the user's signin email (e.g. *abc* in *abc@xyz.com*) is the same string as the password, otherwise returns unauthentticated error. Copy the "logonApi" file from the JitMigrate.conf.json into your conf.json file if you want to use it.