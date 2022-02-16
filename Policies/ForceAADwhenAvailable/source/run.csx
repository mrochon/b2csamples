#r "Newtonsoft.Json"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

private static string[] _social = {
    "gmail.com",
    "live.com",
    "hotmail.com"
};

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    log.LogInformation("Starting GetPolicyDomain.");

    string email = req.Query["email"];
    if(String.IsNullOrEmpty(email))
        return new BadRequestResult();
    var domain_hint = email.Split('@')[1];
    if(String.IsNullOrEmpty(domain_hint))
        return new BadRequestResult();  
    string tp = null;
    if (_social.FirstOrDefault(s => String.Compare(s, domain_hint, true) == 0) == null)
    {    
        using (var http = new HttpClient())
        {
            try
            {
                var json = await http.GetStringAsync($"https://login.microsoftonline.com/{domain_hint}/v2.0/.well-known/openid-configuration");
                var token_endpoint = JObject.Parse(json)["token_endpoint"];
                if (token_endpoint != null)
                    tp = "aad";
            } catch // assuming 400
            {

            }
        }
    }
    log.LogInformation($"GetPolicyDomain returningtp={tp}, domain_hint={domain_hint}.");
    return new JsonResult(new { email, tp, domain_hint});
}