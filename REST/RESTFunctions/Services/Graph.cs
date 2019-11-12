using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace RESTFunctions.Services
{
    public class Graph
    {
        public const string BaseUrl = "https://graph.microsoft.com/v1.0/";
        public Graph(IOptions<ConfidentialClientApplicationOptions> opts)
        {
            var thumb = opts.Value.ClientSecret;
            opts.Value.ClientSecret = String.Empty;
            _app = ConfidentialClientApplicationBuilder
                .CreateWithApplicationOptions(opts.Value)
                .WithCertificate(ReadCertificateFromStore(thumb))
                .Build();
        }
        IConfidentialClientApplication _app;

        public async Task<HttpClient> GetClientAsync()
        {
            var tokens = await _app.AcquireTokenForClient(
                new string[] { "https://graph.microsoft.com/.default" })
                .ExecuteAsync();
            var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer", tokens.AccessToken);
            return http;
        }
        /// <summary>
        /// Reads the certificate
        /// </summary>
        private static X509Certificate2 ReadCertificateFromStore(string thumprint)
        {
            X509Certificate2 cert = null;
            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            var certCollection = store.Certificates;

            // Find unexpired certificates.
            var currentCerts = certCollection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);

            // From the collection of unexpired certificates, find the ones with the correct name.
            var signingCert = currentCerts.Find(X509FindType.FindByThumbprint, thumprint, false);

            // Return the first certificate in the collection, has the right name and is current.
            cert = signingCert.OfType<X509Certificate2>().OrderByDescending(c => c.NotBefore).FirstOrDefault();
            store.Close();
            return cert;
        }
    }
}
