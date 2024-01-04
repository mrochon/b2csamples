#r "Newtonsoft.Json"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    log.LogInformation("Starting errorMessage API.");

    string msg = req.Query["msg"];

    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
    dynamic data = JsonConvert.DeserializeObject(requestBody);
    msg = msg ?? data?.msg;

    return new BadRequestObjectResult(new 
    { 
        version = "1.0.0",
        status = 409,
        userMessage = msg
    }) { StatusCode = 409 };
}