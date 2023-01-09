#r "Newtonsoft.Json"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    string timestamp = req.Query["timestamp"];
    log.LogInformation($"Timestamp:{timestamp}");
    var time = DateTime.Parse(timestamp);
    var elapsedTime = DateTime.UtcNow.Subtract(time);
    // Return 'invalid' if token issued more than 2h ago
    return new OkObjectResult(new { isValid = elapsedTime.TotalMinutes < 120});
}