# Using B2C id_token_hint to initiate the edit journey


## Purpose
Native apps use both browser cookies and refresh tokens to maintain user session
with the token issuer (B2C). The former expire after a much shorter time than the latter. Therefore,
if an app wants to initiate a new journey, e.g. Edit Profile after a day or more of quiescence, browser
cookies are likely to have expired and the journey will need to challenge the user for credentials
even though the app, through refresh cookies could get new tokens without password challenge.

This sample shows how a B2C-issued id_token may be used to initiate the profile edit journey. 
The id_token can be obtained silently as long as the app has a valid refresh token.

To try it out:

1. use the [sign up or sign](https://mrochonb2cprod.b2clogin.com/mrochonb2cprod.onmicrosoft.com/oauth2/v2.0/authorize?p=B2C_1A_IDHINTsignup_signin&client_id=88e4056b-ffce-4660-a3fe-2481e2713197&nonce=defaultNonce&redirect_uri=https%3A%2F%2Foidcdebugger.com%2Fdebug&scope=openid&response_type=id_token&prompt=login) in journey
2. copy the resultant id_token and append it to the following url
3. paste the url in a different browser (or InPrivate version of same browser) to simulate journey initiation without existing SSO cookies.

``https://mrochonb2cprod.b2clogin.com/mrochonb2cprod.onmicrosoft.com/oauth2/v2.0/authorize?p=B2C_1A_IDHINTProfileEdit&client_id=88e4056b-ffce-4660-a3fe-2481e2713197&nonce=defaultNonce&redirect_uri=https%3A%2F%2Foidcdebugger.com%2Fdebug&scope=openid&response_type=id_token&id_token_hint=``

## Method

These policies are based on the [LocalAccounts starter pack](https://github.com/Azure-Samples/active-directory-b2c-custom-policy-starterpack/tree/master/LocalAccounts). 
The TrustFrameworkExtensions.xml 
and ProfileEdit.xml have been modified as per [Azure B2C id_token_hint documentation](https://docs.microsoft.com/en-us/azure/active-directory-b2c/id-token-hint).
As the id_token passed as hint is the id_token issued by B2C itself, no code or changes to policies
intended to handle custom token issuers are needed.