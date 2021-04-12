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
            return new JsonResult(new
            {
                version = "1.0.1",
                status = 409,
                userMessage = "Email cannot be resolved to an IdP"
            });
        }
    }
}
