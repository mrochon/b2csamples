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
        ILogger<GraphClient> _log;
        IConfidentialClientApplication _app;

        public Uri BaseAddress { get => _httpClient.BaseAddress; }

        public async Task<string> GetStringAsync(string segment)
        {
            await AuthorizeClientAsync();
            return await _httpClient.GetStringAsync($"{_httpClient.BaseAddress}{segment}");
        }
        public async Task<HttpResponseMessage> GetAsync(string segment)
        {
            await AuthorizeClientAsync();
            return await _httpClient.GetAsync($"{_httpClient.BaseAddress}{segment}");
        }
        public async Task<HttpResponseMessage> PostAsync(string segment, HttpContent content)
        {
            await AuthorizeClientAsync();
            return await _httpClient.PostAsync($"{_httpClient.BaseAddress}{segment}", content);
       }
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage msg)
        {
            await AuthorizeClientAsync();
            return await _httpClient.SendAsync(msg);
        }
        public async Task<HttpResponseMessage> PatchAsync(string segment, HttpContent content)
        {
            await AuthorizeClientAsync();
            return await _httpClient.PatchAsync($"{_httpClient.BaseAddress}{segment}", content);
        }
        public async Task<IEnumerable<string>> GetAppRoles(string appId, string userObjectId)
        {
            _log.LogInformation("GetAppRoles starting.");
            await AuthorizeClientAsync();
            if (String.IsNullOrEmpty(userObjectId) || String.IsNullOrEmpty(appId))
                throw new ArgumentException();

            _log.LogInformation($"GetAppRoles for userObjectId={userObjectId}&appId={appId}");
            try
            {
                // Cache this!!  https://docs.microsoft.com/en-us/aspnet/core/performance/caching/memory?view=aspnetcore-5.0#:~:text=Cache%20in-memory%20in%20ASP.NET%20Core%201%20Caching%20basics.,8%20Background%20cache%20update.%20...%209%20Additional%20resources
                var json = await GetStringAsync($"applications?$filter=(appId eq '{appId}')&$select=appRoles");
                var appRolesJson = (JArray)JObject.Parse(json)["value"];
                if (appRolesJson.Count() > 0)
                {
                    var appRoles = appRolesJson.First()["appRoles"]
                        .Where(role => role["isEnabled"].Value<bool>())
                        .Select(role => new { id = role["id"].Value<string>(), value = role["value"].Value<string>() });
                    _log.LogInformation($"GetRoles: retrieved {appRoles.Count()} roles");
                    json = await GetStringAsync($"users/{userObjectId}/appRoleAssignments");
                    var roleAssignments = (JArray)JObject.Parse(json)["value"];
                    var roles = roleAssignments
                        .Join(appRoles, ra => ((JObject)ra)["appRoleId"].Value<string>(), role => role.id, (ra, role) => role.value).ToList();
                    return roles;
                }
                return new string[] { };
            }
            catch (Exception ex)
            {
                _log.LogError($"GetAppRoles user/roles exception: {ex.Message}");
                throw;
            }
        }

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
