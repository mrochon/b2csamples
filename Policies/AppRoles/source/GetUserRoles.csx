#r "Newtonsoft.Json"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    log.LogInformation("GetRoles starting.");

    string userObjectId = req.Query["userObjectId"];
    string appId = req.Query["appId"];

    if (String.IsNullOrEmpty(userObjectId) || String.IsNullOrEmpty(appId))
        return new BadRequestObjectResult(new { version = "1.0.0", status = 409, userMessage = "Invalid arguments" });

    log.LogInformation($"GetRoles for userObjectId={userObjectId}&appId={appId}");
    var tenant_id = Environment.GetEnvironmentVariable("B2C:tenant_id");
    var client_id = Environment.GetEnvironmentVariable("B2C:client_id");
    var client_secret = Environment.GetEnvironmentVariable("B2C:client_secret");

    using (var http = new HttpClient())
    {
        try
        {
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
            dynamic tokens = JsonConvert.DeserializeObject(await resp.Content.ReadAsStringAsync());
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ((string)(tokens.access_token)));            
            // Cache this!!  https://docs.microsoft.com/en-us/aspnet/core/performance/caching/memory?view=aspnetcore-5.0#:~:text=Cache%20in-memory%20in%20ASP.NET%20Core%201%20Caching%20basics.,8%20Background%20cache%20update.%20...%209%20Additional%20resources
            var json = await http.GetStringAsync($"https://graph.microsoft.com/v1.0/applications?$filter=(appId eq '{appId}')&$select=appRoles");
            var appRoles = JObject.Parse(json)["value"].First()["appRoles"]
                .Where(role => role["isEnabled"].Value<bool>())
                .Select(role => new { id = role["id"].Value<string>(), value = role["value"].Value<string>() });
            log.LogInformation($"GetRoles: retrieved {appRoles.Count()} roles");
            json = await http.GetStringAsync($"https://graph.microsoft.com/v1.0/users/{userObjectId}/appRoleAssignments");
            var roleAssignments = (JArray)JObject.Parse(json)["value"];
            var roles = roleAssignments
                .Join(appRoles, ra => ((JObject)ra)["appRoleId"].Value<string>(), role => role.id, (ra, role) => role.value).ToList();
            return new OkObjectResult(new { roles });
        }
        catch (Exception ex)
        {
            log.LogError($"user/roles exception: {ex.Message}");
            return new BadRequestObjectResult(new { version = "1.0.0", status = 409, userMessage = ex.Message });
        }
    }
}