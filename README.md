# b2csamples

## Using B2C to support multi-tenant SaaS apps
### Sample
A sample application using policies referenced here is [available here](https://b2cmultitenant.azurewebsites.net) with [source](https://github.com/mrochon/b2csamples/tree/master/Apps/B2CMultiTenant).

### Source
[IEF policies](https://github.com/mrochon/b2csamples/tree/master/Policies/MultiTenant) implementing the two signin/up journeys:
- Signin/up AND create a new application tenant
- Signin/up AND join or continue working with an existing application tenant
These policies [use a number of REST APIs](https://github.com/mrochon/b2csamples/tree/master/REST).
