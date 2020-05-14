# My B2C samples

## This repo has been split up

**In order to provide better setup support for samples in this repo (currently just the Multi-tenancy demo) I am moving each constituent project to its own repo. Here, I will only keep the custom policy xml files and PowerShell scripts and othe rinstructions for deploying complete applications. This is still work in progress.**

## Changes

| Date  | Description  |
|---|---|
| May 5th, 2020  | Provided full [deployment script](https://github.com/mrochon/b2csamples/tree/master/Scripts/MultiTenant) for the multi-tenant sample |
| May 13th, 2020  | Added support for per tenant MFA required option |

## Supporting multi-tenant SaaS apps

### Description
A [sample application](https://b2cmultitenant.azurewebsites.net) illustrating support for multi-tenant SaaS applications using a single B2C Azure AD tenant. All source is in this repo.

**Note:** this sample is **NOT** about [using AAD multi-tenancy](https://docs.microsoft.com/en-us/azure/dotnet-develop-multitenant-applications) to support an application. AAD multi-tenancy is ideal for medium-to-large enterprises who own and manage their own identity infrastructure. This sample is for small enteprises, usually without their own identity infrastructure. It provides support for an application that needs to group it's users into discrete groups, each representing an *application tenant* - a group of people sharing common data in the application. Azure AD B2C allows create their own logins, possibly use some external identity providers (social or work). Using the code provided in this repo, B2C will maintain association between users and *application tenants* and provide that data to your applications when users sign in.

### Source code

The multi-tenant sample consists of three components, each in a separate repo:

1. [IEF policies](https://github.com/mrochon/b2c-mt-rest) implementing several B2C user journeys:
- Signin/up AND create a new application tenant. User can then invite others to this tenant.
- Signin/up AND redeem invitation to become a member of an existing tenant
- Signin as existing member of a tenant.

2. [REST functions](https://github.com/mrochon/b2csamples/tree/master/REST) used by the policies and the sample application: create new application tenant, add members, get member's tenant, create invitation url, get list of tenant members.

3. [Sample multi-tenant application](https://github.com/mrochon/b2c-mt-webapp).

### Setup

Use instructions and scripts provided [here](https://github.com/mrochon/b2csamples/tree/master/Scripts/MultiTenant).

## UserName with batch user upload

[Sample policies](https://github.com/mrochon/b2csamples/tree/master/Policies/UserName) with user journeys supporting B2C tenants in which users are/were created using Graph batch upload.
