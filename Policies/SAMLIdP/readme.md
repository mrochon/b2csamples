# Allows B2C to be used as a SAML IdP


Process:
1. Federate B2C as SAML IdP
2. Invite user using email <original email with @ replaced by '.'>@b2clogin.com
3. User attempts to signin, app must send domain_hint=b2clogin.com to AAD (see below)
4. User is redirected to B2C and signin in with their B2C signin

https://login.microsoftonline.com/7d1abfb9-9f4e-4ec6-8280-722dd7bf9b50/oauth2/v2.0/authorize?client_id=1ef7588f-aa2b-43e9-9961-abf3a4f05e88&nonce=nonce&response_mode=fragment&response_type=id_token&scope=openid+profile+email&sso_nonce=dummyNonce&domain_hint=b2clogin.com

