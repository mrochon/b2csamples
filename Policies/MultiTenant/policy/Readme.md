## Deployment

1. Create API client authorization certficate

```
New-IefPolicyCert MTRestClient
```
2. Copy its thumbprint to appSettings
3. 

```
az ad app create --displayName RESTGraphAccess --required-resource-accesses @manifest.json
("manifest.json" contains the following content)
[
		{
			"resourceAppId": "00000003-0000-0000-c000-000000000000",
			"resourceAccess": [
				{
					"id": "62a82d76-70ea-41e2-9197-370581804d09",
					"type": "Role"
				},
				{
					"id": "7ab1d382-f21e-4acd-a863-ba3e13f7da61",
					"type": "Role"
				}
			]
		}
]
```

4. REST as JWT bearer validator
