Deploy this source as an Azure Function and provide its url and function access code to the conf.json and your B2C policy keys.

Example of call:

```
POST https://<address>
```

Headers:
```
x-functions-key: <key>
Content-type: "application/json
```

Body:
```Json
{"email": "abc@xyz.com", "otp": "12345" }
```
