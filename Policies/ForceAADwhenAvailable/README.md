# Force use of AAD (work account) when available

Requires federation with [AAD to support multi-tenant users](https://docs.microsoft.com/en-us/azure/active-directory-b2c/identity-provider-azure-ad-multi-tenant?pivots=b2c-custom-policy).

Sign-in policy which first asks for user email, determines whether there is an existing Azure AD tenant for
the user's domain and, if it is redirects the user to that tenant. Other users are sent to local signin.

Could probably be extended to likewise redirect users for other, non-AAD federated IdPs.

The REST function used to determine the user's Home Realm (currently AAD only) is ppublicly accessible and referenced in the conf.json file

**Note:** currently the email address used for HRD is not write-protected when displayed later
in the sign-in or sign-up dialogs, i.e. the user could change it. That's a bug - I am waiting to find out why the simple solution (making the fields Readonly in HRDExternsions) is not working
as it should. The alternative is to override both the sign-in and sign-up self-asserted TechnicalProfiles with new ones using a Readonly, new field for that entry.