#r "Newtonsoft.Json"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    log.LogInformation("C# HTTP trigger function processed a request.");

    /* Register this app in your AAD with an Application Permission to Mail.Send (as any user) */
    /* Follow https://docs.microsoft.com/en-us/graph/auth-limit-mailbox-access to restrict the app to specific mailbox(es) */

    var tenant_id = Environment.GetEnvironmentVariable("AzureAD:tenant_id");
    var client_id = Environment.GetEnvironmentVariable("AzureAD:client_id");
    var client_secret = Environment.GetEnvironmentVariable("AzureAD:client_secret");
    var sender_email = Environment.GetEnvironmentVariable("AzureAD:sender_email");

    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
    dynamic data = JsonConvert.DeserializeObject(requestBody);
    string email = data?.email;
    string otp = data?.otp;

    if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(otp))
        return new BadRequestObjectResult(new { version = "1.0.0", status = 409, userMessage = "Invalid arguments" });
    log.LogInformation($"Sending OTP to {email}.");

    var http = new HttpClient();
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
    log.LogInformation("Access token received");
    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ((string)(tokens.access_token)));
    var msg = new
    {
        message = new
        {
            subject = "OTP Code",
            body = new
            {
                contentType = "Text",
                content = $"Please use {otp} to confirm your email address"
            },
            toRecipients = new[] { new { emailAddress = new { address = email } } }
        },
        saveToSentItems = false
    };    
    resp = await http.SendAsync(
        new HttpRequestMessage(HttpMethod.Post, $"https://graph.microsoft.com/v1.0/users/{sender_email}/sendMail")
        {
            Content = new StringContent(JsonConvert.SerializeObject(msg), System.Text.Encoding.UTF8, "application/json")
        });
    if (resp.IsSuccessStatusCode)
        return new OkResult();
    else
        log.LogError(await resp.Content.ReadAsStringAsync());        
    
    return new BadRequestObjectResult(new { version = "1.0.0", status = 409, userMessage = "Unable to send msg." });
}