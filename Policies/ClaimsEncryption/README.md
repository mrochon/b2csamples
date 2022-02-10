# Claims Encryption
## Purpose
Provides Azure B2C JWT token confidentiality by encrypting **selected** in the issued token. Clients use a back-channel to call an API to decrypt the claims.

## Encryption

To add the xml policy to your existing policy set you can use [IefPolicies module](https://www.powershellgallery.com/packages/IefPolicies) with the following command

```PowerShell
Add-IefPoliciesSample Claimsencryption -owner mrochon -repo b2csamples
```

Modify the list of InputClaims sent to the encryption function as per your requirements. You may also need to adjust the journey step order numbers depending on how many steps you already have in your SignUpSignIn journey.

Deploy the [encryption function](https://github.com/mrochon/b2csamples/tree/master/Policies/ClaimsEncryption/source) as an Azure Function. The xml policy currently expects it to allow anonymous access. Set the url of the deployed function in your conf.json file.

Create a self-signed X509 certificate and deploy its **public** key to the Azure function. The public key of an X509 certificate, whose private key is available to the client application (application needing the decrypted claims), must be deployed with the REST function. Its thumbprint should be added to the service configuration as *EncryptionCertThumbprint*.

Function provided in the source folder encrypts a JSON object with the public key of an X509 certificate and returns a JSON object with a single property (*encrypted*) with the encrypted value (Base64 encoded). This operation is used by B2C custom policies to encrypt selected claims.

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

