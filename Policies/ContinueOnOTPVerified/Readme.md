# Continue pwd reset on OTP verified

Custom policy with custom html page showing how to do OTP verification (in the password reset subjourney) and immediately
proceed to the next screen if the OTP is valid. Avoids having the user click both the Verify button and Continue button.

## Deployment
Deploy the html file to your own, public facing web server and modify the url in the xml file (Contentdefinition) to point
to that file.

If you are using the [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies), to add this policy to your 
policy set folder:

```PowerShell
Add-IefPoliciesSample ContinueOnOTPVerified -owner mrochon -repo b2csamples
```