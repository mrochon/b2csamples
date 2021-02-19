# Force use of AAD (work account) when available

Sign-in policy which first asks for user email, determines whether there is an existing Azure AD tenant for
the user's domain and, if it is redirects the user to that tenant. Other users are sent to local signin.

Could probably be extended to likewise redirect users for other, non-AAD federated IdPs.