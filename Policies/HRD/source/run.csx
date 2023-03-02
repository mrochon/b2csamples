#r "Newtonsoft.Json"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

private static Dictionary<string,string> _idpDomains = new Dictionary<string,string> {
    { "gmail.com", "gmail" },
    { "live.com", "msa" },
    { "hotmail.com", "msa" },
    { "outlook.com", "msa" }
};

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    log.LogInformation("Starting GetPolicyDomain.");

    string email = req.Query["email"];
    if(String.IsNullOrEmpty(email))
        return new BadRequestResult();
    log.LogInformation($"   for {email}");
    var domain_hint = email.Split('@')[1];
    if(String.IsNullOrEmpty(domain_hint))
        return new BadRequestResult();  
      
    if (_idpDomains.ContainsKey(domain_hint))
    {
        log.LogInformation($"Returning (true, {_idpDomains[domain_hint]}, {domain_hint}).");
        return new JsonResult(new { isKnownDomain = true, idp = _idpDomains[domain_hint], domain_hint, email});  
    } else
    {    
        using (var http = new HttpClient())
        {
            try
            {
                var json = await http.GetStringAsync($"https://login.microsoftonline.com/{domain_hint}/v2.0/.well-known/openid-configuration");
                var token_endpoint = JObject.Parse(json)["token_endpoint"];
                if (token_endpoint != null)
                {
                    log.LogInformation($"Returning (true, aad, {domain_hint}).");
                    return new JsonResult(new { isKnownDomain = true, idp = "aad", domain_hint, email});                    
                }
            } catch // assuming 400
            {

            }
        }
    }
    log.LogInformation($"Returning (false, local, {domain_hint}).");
    return new JsonResult(new { isKnownDomain = false, idp = "unknown", domain_hint, email});  
}