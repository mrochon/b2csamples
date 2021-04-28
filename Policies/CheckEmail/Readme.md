# ensure users do not use work emails for local accounts

The signup/sign policy in this set ensures that users do not create local accounts
using email addresses with specified domains. The error message displayed during signup or signin should that happens directs
the suers to select the UI button that takes them to their federated IdP.

To view this policy in operation in operation use this url:

`https://mrochonb2cprod.b2clogin.com/mrochonb2cprod.onmicrosoft.com/oauth2/v2.0/authorize?p=B2C_1A_CheckEmailsignup_signin&client_id=88e4056b-ffce-4660-a3fe-2481e2713197&nonce=defaultNonce&redirect_uri=https%3A%2F%2Foidcdebugger.com%2Fdebug&scope=openid&response_type=id_token&prompt=login

enter any email address containg meraridom.com as domain in either the signin UI (use any fake pwd) or the signup UI.