using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace B2CRestApis.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OIDCController : ControllerBase
    {
        private readonly ILogger<Identity> _logger;
        public OIDCController(ILogger<Identity> logger)
        {
            _logger = logger;
        }
        [HttpGet("userinfo")]
        public async Task<ActionResult> UserInfo([FromQuery] string token)
        {
            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var json = await http.GetStringAsync("https://iddev-priorityhealtheid.cs41.force.com/phim/services/oauth2/userinfo");
                var id = JObject.Parse(json)["custom_attributes"]["GDSMBRID"].Value<string>();
                return new JsonResult(new { id });
            }
        }
    }
}
