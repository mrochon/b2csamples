# Allows B2C to be used as a SAML IdP for AAD Direct Federation

## Background
Azure AD (enterprise) supports [federation to external IdPs](https://docs.microsoft.com/en-us/azure/active-directory/external-identities/direct-federation) to support external user identities. This sample shows how use this feature to federate an Azure B2C tenant as IdP to an AAD.

## Basic operation
This sample uses standard setup for using B2C as SAML IdP modified to issue user's email address transformed to use a fake domain name. In this sample email 'abc@gmail.com' becomes 'abc.gmail.com@b2clogin.com'. AAD inviters need to use this invitation to create a B2B invite. They can then modify the user's email address back to the original in the user's record in AAD. (be aware that there is a period of a few minutes after the invitation is created during which the portal does not allow editing of the user data).

In order for the B2C users to sign in to AAD managed resources, either the resource **must** use *domain_hint=the fake domain* or the user will need to use the faked email address to help AAD with HRD.

## Setup
1. IEF policies in this folder implement a signin/up journey issuing a SAML token as per [documentation](https://docs.microsoft.com/en-us/azure/active-directory-b2c/identity-provider-generic-saml?tabs=windows&pivots=b2c-custom-policy).
2. If using the [IefPolicies PS module](https://www.powershellgallery.com/packages/IefPolicies), modify the conf.json file below with your settings:
    2.1. Prefix - name prefix you want your policies to have when loaded in B2C
    2.1. SAMLIssuer - SAML issuer id you want to use for your B2C
    2.3. GoogleClientId - OAuth2 client id for Google federation (remove if not needed)
    2.4. B2CEmailDomain - a fake domain you will use when inviting B2C users to AAD
2. These policies also add some claims transformations to:
    2.1. Preserve local user's signin id, which is assumed to be an email for issuance in the token
    2.2. Modify that email address to use a format that can be resolved in AAD. In these policies the format replaces the original '@' with a '.' (period) and appends '@b2clogin.com'.
3. The resulting email is included in the SAML token as "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", as required by AAD.
4. Setup your AAD to use B2C as SAML IdP. The metadata you will need to provide to AAD can be found at
`https://<tenant-name>.b2clogin.com/<tenant-name>.onmicrosoft.com/<saml signin policy-name>/samlp/metadata`
5. Modify your applications include a domain_hint when needing to sign in the B2B users from B2C. (alternatively, these users would need to use transformed email address when challenged by AAD), e.g.:
[https://login.microsoftonline.com/7d1abfb9-9f4e-4ec6-8280-722dd7bf9b50/oauth2/v2.0/authorize?client_id=1ef7588f-aa2b-43e9-9961-abf3a4f05e88&nonce=nonce&response_mode=fragment&response_type=id_token&scope=openid+profile+email&sso_nonce=dummyNonce&domain_hint=b2clogin.com](https://login.microsoftonline.com/7d1abfb9-9f4e-4ec6-8280-722dd7bf9b50/oauth2/v2.0/authorize?client_id=1ef7588f-aa2b-43e9-9961-abf3a4f05e88&nonce=nonce&response_mode=fragment&response_type=id_token&scope=openid+profile+email&sso_nonce=dummyNonce&domain_hint=b2clogin.com)
(Note: you can use the above link to start the process but since I have not invited you to my AAD, you will get an error at the end of the process saying just that. However, that proves that the B2B process has received the correct token from B2C).

## User invitation process
  
2. Invite user using email <original email with @ replaced by '.'>@<fake domain name>, e.g. user1.gmail.com@b2clogin.com.
3. User attempts to signin, app must send domain_hint=b2clogin.com to AAD (see below)
4. User is redirected to B2C and signs in (or -up) in to B2C
4. SAML token is sent back to AAD



