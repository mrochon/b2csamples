# Invitation using client_assertion

## Purpose
These policies use a JWT token to initiate a signup operation. The url is created using
[one of the REST API](https://github.com/mrochon/b2csamples/tree/master/REST): User.Invite.

This sample uses the client_assertion request format, rather than [id_token_hint alternative](https://github.com/azure-ad-b2c/samples/tree/master/policies/invite)
 since the latter requires additional publi
endpoints to be exposed by the inviting application (.well-known and jwks).

## Sample operation

Use the following url to obtain an access token to the REST API (you will have to signup for a new account in my tenant if you do not yet have one):

[Signin and get access token](https://mrochonb2cprod.b2clogin.com/mrochonb2cprod.onmicrosoft.com/oauth2/v2.0/authorize?p=B2C_1_BasicSUSI&client_id=88e4056b-ffce-4660-a3fe-2481e2713197&nonce=defaultNonce&redirect_uri=https%3A%2F%2Foidcdebugger.com%2Fdebug&scope=openid%20https%3A%2F%2Fmrochonb2cprod.onmicrosoft.com%2Fb2crestapis%2FUser.Invite&response_type=id_token%20token&prompt=login)

Use Postman, CURL or similar tool make POST request to:

https://b2crestapis.azurewebsites.net/user/oauth2/invite

Using the access token obtained above as Authorization bearer token, with the following request body:

```json
{"inviteEmail":"email@domain.com","postRedeemAppId": "88e4056b-ffce-4660-a3fe-2481e2713197", "postRedeemUrl": "https://oidcdebugger.com/debug"}
```

You should get a response that looks as follows:

```
https://mrochonb2cprod.b2clogin.com/cf6c572c-c72e-4f31-bd0b-75623d040495/B2C_1A_INVITEsignup_invitation/oauth2/v2.0/authorize?client_id=88e4056b-ffce-4660-a3fe-2481e2713197&login_hint=mrochon@microsoft.com&response_mode=form_post&nonce=defaultNonce&redirect_uri=https://oidcdebugger.com/debug&scope=openid&response_type=id_token&client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion=eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6Im1yb2Nob25AbWljcm9zb2Z0LmNvbSIsIm5iZiI6MTYyMzE5MjAwMCwiZXhwIjoxNjIzMTk1NjAwLCJpc3MiOiJtcm9jaG9uYjJjcHJvZCIsImF1ZCI6Im1yb2Nob25iMmNwcm9kIn0.9e4BRNbL7rSc5DulCDDJF7fSXGHkSF-XSKSyfbWuNfA
```

This is what the invited user should execute through the browser.