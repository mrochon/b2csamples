#r "Newtonsoft.Json"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Security.Cryptography;

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    log.LogInformation("Encrypt request.");
    string data = await new StreamReader(req.Body).ReadToEndAsync();
    var thumbprint = Environment.GetEnvironmentVariable("EncryptionCertThumbprint");
    var cert = GetCertificate(thumbprint, log);
    var dataBytes = Encoding.ASCII.GetBytes(data);
    var encrypted = cert.PublicKey.GetRSAPublicKey().Encrypt(dataBytes, RSAEncryptionPadding.OaepSHA256);
    var responseMessage = new { encrypted };
    return new OkObjectResult(responseMessage);
}

private static X509Certificate2 GetCertificate(string thumbprint, ILogger log)
{
    X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
    try
    {
        store.Open(OpenFlags.ReadOnly);
        log.LogInformation("Enumerating certificates");
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


