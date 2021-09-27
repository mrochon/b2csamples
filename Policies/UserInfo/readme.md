# UserInfo endpoint demo

Based on Local User starter pack and [UserInfo custom policy docs](https://docs.microsoft.com/en-us/azure/active-directory-b2c/userinfo-endpoint?pivots=b2c-custom-policy).

Use standard OIDC request with this policy. Then:

GET https://yourtenant.b2clogin.com/yourtenant.onmicrosoft.com/policy-name/openid/v2.0/userinfo
Authorization Bearer <id_token>