#r "Newtonsoft.Json"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    log.LogInformation("C# HTTP trigger function processed a request.");

    string signInName = req.Query["signInName"];
    string password = req.Query["password"];    

    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
    dynamic data = JsonConvert.DeserializeObject(requestBody);
    signInName = signInName ?? data?.signInName;
    password = password ?? data?.password; 
    if (!String.IsNullOrEmpty(signInName) && String.Compare(signInName, password) == 0)  
        return  new OkObjectResult(new { displayName = $"{signInName}, {signInName}"});

    return new BadRequestObjectResult(new { version = "1.0.0", status = 409, userMessage = "Authentication failed" });
}