using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MR.B2C
{
    public class AuthnMethods
    {
        private readonly ILogger<GetUserRoles> _logger;
        private static DateTime _expires_at;
        private static string _access_token = String.Empty;

        public AuthnMethods(ILogger<GetUserRoles> logger)
        {
            _logger = logger;
        }

        [Function("DeleteAuthnMethod")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            _logger.LogDebug("DeleteAuthnMethod starting.");

            string objectId = req.Query["objectId"];

            if (String.IsNullOrEmpty(objectId))
                return new BadRequestObjectResult(new { version = "1.0.0", status = 409, userMessage = "Invalid arguments" });

            _logger.LogDebug($"DeleteAuthnMethod for objectId={objectId}");
            var tenant_id = Environment.GetEnvironmentVariable("tenant_id");
            var client_id = Environment.GetEnvironmentVariable("client_id");
            var client_secret = Environment.GetEnvironmentVariable("client_secret");

            using (var http = new HttpClient())
            {
                try
                {
                    _logger.LogDebug($"Graph client: {client_id}");
                    var limit_time = DateTime.UtcNow.AddMinutes(-5);
                    _logger.LogDebug($"Renew by: {limit_time}; Current expiry: {_expires_at}");
                    if(limit_time.CompareTo(_expires_at) >= 0)
                    {
                        _logger.LogDebug($"Getting new token");
                        var resp = await http.PostAsync($"https://login.microsoftonline.com/{tenant_id}/oauth2/v2.0/token",
                            new FormUrlEncodedContent(new List<KeyValuePair<String, String>>
                            {
                                new KeyValuePair<string,string>("client_id", client_id),
                                new KeyValuePair<string,string>("scope", "https://graph.microsoft.com/.default"),
                                new KeyValuePair<string,string>("client_secret", client_secret),
                                new KeyValuePair<string,string>("grant_type", "client_credentials")
                            }));
                        if (!resp.IsSuccessStatusCode)
                            return new BadRequestObjectResult(new { version = "1.0.0", status = 409, userMessage = "Authorization failure" });
                        var tokenResponse = JsonNode.Parse(await resp.Content.ReadAsStringAsync());
                        _logger.LogDebug("Token Json received");
                        _access_token = tokenResponse!["access_token"]!.GetValue<string>();
                        _expires_at = DateTime.UtcNow.AddSeconds(tokenResponse["expires_in"]!.GetValue<int>());
                        _logger.LogDebug($"New token obtained. Expires at: {_expires_at}");
                    }
                    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _access_token);            
                    // Cache this!!  https://docs.microsoft.com/en-us/aspnet/core/performance/caching/memory?view=aspnetcore-5.0#:~:text=Cache%20in-memory%20in%20ASP.NET%20Core%201%20Caching%20basics.,8%20Background%20cache%20update.%20...%209%20Additional%20resources
                    _logger.LogDebug("Token obtained");

                    //TODO: implement throttling support
                    var json = await http.GetStringAsync($"https://graph.microsoft.com/v1.0/users/{objectId}/authentication/softwareOathMethods");
                    var value = ((JsonArray)JsonNode.Parse(json)!["value"])!.FirstOrDefault();
                    if(value == null)
                        return new OkResult(); // no TOTPs
                    var id = value["id"].GetValue<string>();
                    var res = await http.DeleteAsync($"https://graph.microsoft.com/v1.0/users/{objectId}/authentication/softwareOathMethods/{id}");
                    if(!res.IsSuccessStatusCode)
                        return new BadRequestObjectResult(new { version = "1.0.0", status = 409, userMessage = "Failed to delete TOTP" });
                    return new OkResult();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"user/roles exception: {ex.Message}");
                    return new BadRequestObjectResult(new { version = "1.0.0", status = 409, userMessage = ex.Message });
                }
            }
        }
    }
}
