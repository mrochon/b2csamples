# Claims Encryption
## Purpose
Provides Azure B2C JWT token confidentiality by encrypting **selected** in the issued token. Clients use a back-channel to call an API to decrypt the claims.

## Operation
This system consists of two main components: a web application providing the encryption/decryption operations and B2C policies calling the encryption operation during a journey.

### REST functions

The public key of an X509 certificate, whose private key is available to the client application (application needing the decrypted claims), must be deployed with the REST function. Its thumbprint should be added to the service configuration as *EncryptionCertThumbprint*.

Function provided in the source folder encrypts a JSON object with the public key of an X509 certificate and returns a JSON object with a single property (*encrypted*) with the encrypted value (Base64 encoded). This operation is used by B2C custom policies to encrypt selected claims.

**Request**

    POST /encrypt
    Content-type: application/json
    Accept: application/json

    {"attr1":"value1","attr2":"value2"}

**Response**

    {"encrypted":"CfDJ8OKS0TfF....27hGvaJ0kU3oTTl"}

### Decryption

Client application can use the following code to decrypt the *encrypted* claim. *certificateThumbprint should reference a certificate available to the app.

```CS
...
var encrypted = User.FindFirst("encrypted").Value;
var bytes = Convert.FromBase64String(encrypted);
var cert = Decryptor.GetCertificate(certificateThumbprint);
var decrypted = cert.GetRSAPrivateKey().Decrypt(bytes, RSAEncryptionPadding.OaepSHA256);
var decryptedStr = Encoding.Default.GetString(decrypted);
var claims = JsonObject.Parse(decryptedStr);

public static X509Certificate2 GetCertificate(string thumbprint)
{
    X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
    try
    {
        store.Open(OpenFlags.ReadOnly);
        var col = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
        if (col == null || col.Count == 0)
        {
            return null;
        }
        return col[0];
    }
    finally
    {
        store.Close();
    }
}
```

### B2C custom policy

*CryptoExtensions.xml* modifies the standard, **local account** sign-up/-in journey to encrypt user's display name. To use it with other starter packs, modify the order number of the journey steps in this extension to coincide with the number of the last two steps in the journey you want to encrypt the claims.



