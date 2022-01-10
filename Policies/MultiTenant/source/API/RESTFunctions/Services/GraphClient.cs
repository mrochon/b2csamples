using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace RESTFunctions.Services
{
    public class GraphClient
    {
        public GraphClient(HttpClient httpClient, IOptions<ConfidentialClientApplicationOptions> opts, ILogger<GraphClient> log)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
            _log = log;
            var thumb = opts.Value.ClientSecret;
            var cert = ReadCertificateFromStore(thumb);
            if (cert != null)
            {
                opts.Value.ClientSecret = String.Empty;
                _app = ConfidentialClientApplicationBuilder
                    .CreateWithApplicationOptions(opts.Value)
                    .WithCertificate(cert)
                    .Build();
            } else
                _app = ConfidentialClientApplicationBuilder
                    .CreateWithApplicationOptions(opts.Value)
                    //.WithClientSecret(thumb)
                    .Build();
        }
        private readonly HttpClient _httpClient;
        public async Task<HttpClient> CreateClient()
        {
            //if (_httpClient.DefaultRequestHeaders.Authorization == null) // could cause auth errors if header stays for too long
                await AuthorizeClientAsync();
            return _httpClient; 
        }

        ILogger<GraphClient> _log;
        IConfidentialClientApplication _app;

        public async Task AuthorizeClientAsync()
        {
            var tokens = await _app.AcquireTokenForClient(
                new string[] { "https://graph.microsoft.com/.default" })
                .ExecuteAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer", tokens.AccessToken);
        }
        /// <summary>
        /// Reads the certificate
        /// </summary>
        private static X509Certificate2 ReadCertificateFromStore(string thumprint)
        {
            X509Certificate2 cert = null;
            try
            {

                using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                {
                    store.Open(OpenFlags.ReadOnly);
                    var certCollection = store.Certificates;

                    // Find unexpired certificates.
                    var currentCerts = certCollection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);

                    // From the collection of unexpired certificates, find the ones with the correct name.
                    var signingCert = currentCerts.Find(X509FindType.FindByThumbprint, thumprint, false);

                    // Return the first certificate in the collection, has the right name and is current.
                    cert = signingCert.OfType<X509Certificate2>().OrderByDescending(c => c.NotBefore).FirstOrDefault();
                    store.Close();
                }
            } catch (Exception ex)
            {
                Debug.WriteLine($"ReadCertificateFromStore exception: {0}", ex.Message);
            }
            return cert;
        }
    }
}
