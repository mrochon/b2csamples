# Token Refresh - Custom Expiry time

## Starter pack

May be added to any starter pack

## Purpose

Implements a custom refresh token issuance process which uses a custom refresh token expiration time, shorter than 24h (min for B2C)

## Components

1. SignupSignIn journey with custom token refresh journey definition and associated custoom Technical Profiles

2. REST function which compares token issuance time to some fixed limit. Deployable to Azure Functions.
