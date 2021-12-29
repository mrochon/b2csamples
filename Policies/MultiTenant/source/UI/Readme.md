## Descrription

A simple React Single Page App demonstrating some of the features of the B2C multi-tenant sample.

The application uses a single Azure B2C instance and allows users to sign up, define their own *application tenants*, invite other
users to become members and see that data reflected in the tokens received by the application from the B2C.

## Installation

1. clone the source to your folder
2. modify the contents of the *deployment* object in the authConfig.js file
3. npm run build
4. you can deploy to Azure static web server [using VSCode extension](https://jyoo.github.io/deploying-react-spa-in-10-minutes-using-azure).