using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using B2CRestApis.Services;

namespace B2CRestApis.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class HRD : ControllerBase
    {
        private readonly ILogger<HRD> _logger;
        public HRD(ILogger<HRD> logger)
        {
            _logger = logger;
        }

        public async Task<ActionResult> GetDomainHint(string email)
        {
            var http = new HttpClient();
            var json = await http.GetStringAsync($"https://login.windows.net/common/UserRealm/{email}?api-version=2.0");
            try
            {
                var domain_hint = JsonDocument.Parse(json).RootElement.GetProperty("DomainName").GetString();
                if (String.Compare(domain_hint, "live.com") != 0)
                    return new JsonResult(new { email, tp = "aad", domain_hint });
            }
            catch
            {
            }
            return this.ErrorResponse(409, "Email cannot be resolved to an IdP");
        }
        private static string[] _IdPDomains = { "gmail.com" };
        [HttpGet("GetDomainHint2")]
        public async Task<ActionResult> GetDomainHint2([FromQuery] string email)
        {
            var http = new HttpClient();
            var domain_hint = email.Split('@')[1];
            if (_IdPDomains.Contains(domain_hint))
                return new JsonResult(new { email, tp = "other", domain_hint });
            try
            {
                var json = await http.GetStringAsync($"https://login.microsoftonline.com/{domain_hint}/v2.0/.well-known/openid-configuration");
                JsonDocument.Parse(json).RootElement.GetProperty("token_endpoint");
                return new JsonResult(new { email, tp = "aad", domain_hint });
            }
            catch
            {
            }
            return new JsonResult(new { email, tp = "local",  domain_hint});
        }
        private static string[] FederatedDomains = { "Meraridom.com" };
        [HttpGet("DeclineWorkEmail")]
        public ActionResult DeclineWorkAddress([FromQuery] string email)
        {
            if (String.IsNullOrEmpty(email))
            {
                _logger.LogError($"DeclineWorkemail received empty email");
                return this.ErrorResponse(409, "Missing email address");
            }
            var inp = email.ToLowerInvariant();
            var existing = FederatedDomains.Where(d => inp.Contains(d.ToLowerInvariant())).FirstOrDefault();
            if (existing != null)
            {
                _logger.LogError($"{email} is invalid");
                return this.ErrorResponse(409, $"Please use the {existing.Split('.').First()} Employee login button");
            }
            _logger.LogInformation("DeclineWorkEmail returning OK");
            return Ok();
        }
    }
}
