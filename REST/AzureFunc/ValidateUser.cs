using System.ComponentModel;
using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MR.AzureFunc
{
    public class ValidateUser
    {
        private readonly ILogger<ValidateUser> _logger;

        public ValidateUser(ILogger<ValidateUser> logger)
        {
            _logger = logger;
        }

        [Function("ValidateUser")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("ValidateUser starting.");
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            _logger.LogInformation($"ValidateUser received: {body}");
            var user = JsonSerializer.Deserialize<User>(body);
            if((user == null) || String.IsNullOrEmpty(user.displayName) || String.IsNullOrEmpty(user.extension_9fa255f4f18847feb0c166c6f2ad769a_EmployeeID))
                return new BadRequestObjectResult(new { version = "1.0.0", status = 409, userMessage = "Invalid or missing user data." });
                _logger.LogInformation($"ValidateUser user: {user.displayName}/{user.extension_9fa255f4f18847feb0c166c6f2ad769a_EmployeeID  }");
            if(user.displayName.EndsWith(user.extension_9fa255f4f18847feb0c166c6f2ad769a_EmployeeID))
                return new OkObjectResult(new { version = "1.0.0", status = 200, userMessage = "User validated" });
            return new BadRequestObjectResult(new { version = "1.0.0", status = 409, userMessage = "Your signup data is invalid." });
        }
    }
    public class User
    {
        public string? displayName { get; set; }
        public string? givenName { get; set; }
        public string? surname { get; set; }
        public string? extension_9fa255f4f18847feb0c166c6f2ad769a_EmployeeID { get; set; }
    }
}
