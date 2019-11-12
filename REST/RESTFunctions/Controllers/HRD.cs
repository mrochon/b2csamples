using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using RESTFunctions.Services;

namespace RESTFunctions.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HRD : ControllerBase
    {
        [HttpGet("isAAD")]
        public IActionResult GetFromUserCode(string userCode)
        {
            try
            {
                return new JsonResult(new { aadDomain = CodeDomainMapping[userCode] });
            }
            catch
            {
                return new OkResult();
            }
        }
        static Dictionary<string, string> CodeDomainMapping = new Dictionary<string, string>()
        {
            {"jda", "jda.com" },
            {"abc", "microsoft.com" },
            {"xyz", "meraridom.com" }
        };

    }
}
