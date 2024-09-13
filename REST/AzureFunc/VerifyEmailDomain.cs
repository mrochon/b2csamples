using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MR.AzureFunc
{
    public class VerifyEmailDomain
    {
        private List<string> _domains = new List<string>()
        {
            "microsoft.com",
            "sleepnumber.com"
        };

        private readonly ILogger<VerifyEmailDomain> _logger;

        public VerifyEmailDomain(ILogger<VerifyEmailDomain> logger)
        {
            _logger = logger;
        }

        [Function("VerifyEmailDomain")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            _logger.LogInformation("VerifyEmailDomain starting.");
            try
            {
                var email = req.Query.FirstOrDefault(p => p.Key == "email").Value[0];
                var domain = email!.Split('@')[1];
                if (_domains.Contains(domain))
                {
                    _logger.LogInformation("Returning OK.");
                    return new OkResult();
                }
            } catch(Exception ex)
            {
                _logger.LogError("Returning: bad arguments.");
                return new BadRequestObjectResult(new { version = "1.0.0", status = 409, userMessage = ex.Message });
            }
            _logger.LogError("Returning access denied because of invalid domain.");
            return new BadRequestObjectResult(new { version = "1.0.0", status = 409, userMessage = "Access denied." });
        }
    }
}
