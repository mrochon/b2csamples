# Call Graph

Uses Client Creds call to get an access token to MS Graph and then calls an API using that token

## Base starter pack

The policy as-is is based on the LocalOnly starter pack. To use it with other packs, change the orchestration steps
to start with the sequence number of the SignInSignUp journey, e.g. changge it 7 for SocialAndLocal. 

## Adding sample to your policy set

Use [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies) with the following command

```PowerShell
Add-IefPoliciesSample CallGraph -owner mrochon -repo b2csamples
```
