# Invitation using client_assertion

This policy can be used with any starter pack.

These policies use a JWT token to initiate a signup operation. A .NET C# console ap with
a service class to create the invitation url is included in the source directory.

This sample uses the client_assertion request format, rather than [id_token_hint alternative](https://github.com/azure-ad-b2c/samples/tree/master/policies/invite)
 since the latter requires additional public
endpoints to be exposed by the inviting application (.well-known and jwks).

