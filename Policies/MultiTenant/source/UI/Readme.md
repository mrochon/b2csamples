## Description

A simple React Single Page App demonstrating some of the features of the B2C multi-tenant sample.

The application uses a single Azure B2C instance and allows users to sign up, define their own *application tenants*, invite other
users to become members and see that data reflected in the tokens received by the application from the B2C.

## Installation

1. clone the source to your folder
2. modify the contents of the *deployment* object in the authConfig.js file
3. npm run build
4. you can deploy to Azure static web server [using VSCode extension](https://jyoo.github.io/deploying-react-spa-in-10-minutes-using-azure).


## Issues

1. Do not ask for refresh for a different journey than original signin request: {"error":"invalid_grant","error_description":"AADB2C90088: The provided grant has not been issued for this endpoint. Actual Value : B2C_1A_V2SignIn and Expected Value : B2C_1A_V2NewTenant\r\nCorrelation ID: f9ba829c-a81f-4ef9-ad61-e1f17fb38727\r\nTimestamp: 2022-01-02 18:14:17Z\r\n"}
