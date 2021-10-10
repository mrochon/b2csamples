# Invitation using client_assertion

These policies use a custom JWT token to initiate a signup operation. There are two separate journeys:

1. Inviting local accounts only - can be used with any starter pack though most appropriate for LocalOnly. The journey allows only new sign up (no sign in).
2. Inviting federating accounts only - can **not** be used with local-only starter pack. The journey will validate that the email the user signed in with is the same one as the invited email. A user may use a federated account
that was previously signed up.

Source directory contains a .NET Core console app creating a url with embedded JWT token that an invited user can use to redeem the invitation. It requires that the B2C tenant be set up with a symmetric signing key. The key is referenced with the name B2_1A_InvitationSigningKey in the policy files.

This sample uses the client_assertion request format, rather than [id_token_hint alternative](https://github.com/azure-ad-b2c/samples/tree/master/policies/invite)
 since the latter requires additional public
endpoints to be exposed by the inviting application (.well-known and jwks).

