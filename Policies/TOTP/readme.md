# TOTP Extensions

Adds TOTP (Authenticator) extensions to a policy pack. 

May be used with any starter pack.

To add this sample to the your starter pack:
```PowerShell
Add-IefPoliciesSample TOTP -owner mrochon -repo b2csamples
```
Then integrate it into your journeys by overriding the last two steps of the journeys with (renumber to fit your step sequence):

```xml
      <OrchestrationStep Order="7" Type="InvokeSubJourney">
        <JourneyList>
          <Candidate SubJourneyReferenceId="UseTOTP" />
        </JourneyList>
      </OrchestrationStep>   
      <OrchestrationStep Order="8" Type="SendClaims" CpimIssuerTechnicalProfileReferenceId="JwtIssuer" />      
    </OrchestrationSteps>       
```
