using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using B2CRestApis.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace B2CRestApis.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Identity : ControllerBase
    {
        private readonly ILogger<Identity> _logger;
        private readonly IHttpClientFactory _clientFactory;

        public Identity(ILogger<Identity> logger, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _clientFactory = clientFactory;
        }
        [HttpGet("test")]
        public string Test()
        {
            return "OK";
        }

        /// <summary>
        /// If userid and pwd are equal returns success with data pointing to an existing B2C user. Otherwise, fails.
        /// </summary>
        /// <param name="cred"></param>
        /// <returns></returns>
        [HttpPost("auth")]
        public async Task<ActionResult> Authenticate([FromBody] Credential cred)
        {
            if (string.Compare(cred.userId, cred.password) == 0)
            {
                string id = String.Empty;
                using (var http = _clientFactory.CreateClient("B2C"))
                {
                    //var http = _clientFactory.CreateClient("B2C");
                    var resp = await http.SendAsync(
                        new HttpRequestMessage(HttpMethod.Get, http.BaseAddress + $"users?$select=id&$filter=identities/any(c:c/issuerAssignedId eq '{cred.userId}' and c/issuer eq 'mrochonb2cprod.onmicrosoft.com')"));
                    if (resp.IsSuccessStatusCode)
                    {
                        var users = (JArray)JObject.Parse(await resp.Content.ReadAsStringAsync())["value"];
                        if (users.Count == 1)
                            id = users[0]["id"].Value<string>();
                        else
                        {
                            var identity = new { signInType = "userName", issuer = "mrochonb2cprod.onmicrosoft.com", issuerAssignedId = cred.userId };
                            var user = new
                            {
                                displayName = "Uknown",
                                identities = new[] { identity },
                                passwordProfile = new
                                {
                                    cred.password,
                                    forceChangePasswordNextSignIn = false
                                },
                                passwordPolicies = "DisablePasswordExpiration,DisableStrongPassword"
                            };
                            var json = JsonConvert.SerializeObject(user);
                            resp = await http.SendAsync(
                                new HttpRequestMessage(HttpMethod.Post, http.BaseAddress + "users")
                                {
                                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                                });
                            if (resp.IsSuccessStatusCode)
                                id = JObject.Parse(await resp.Content.ReadAsStringAsync())["id"].Value<string>();
                            else
                                return new StatusCodeResult(500);
                        }
                    } else
                        throw new Exception(resp.ReasonPhrase);

                    return new JsonResult(new
                    {
                        cred.userId,
                        objectId = id
                    });
                }
            }
            else
                return StatusCode(404, new ErrorMsg { userMessage = "User id or password is invalid" });
        }
        private static readonly string emailPattern = @"^((\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*)\s*";
        private static readonly string ssnPattern = @"^\d{3}-\d{2}-\d{4}$";
        [HttpGet]
        public object GetIdType([FromQuery] string id)
        {
            if (Regex.IsMatch(id, emailPattern))
                return new { idType = "email" };
            if (Regex.IsMatch(id, ssnPattern))
                return new { idType = "ssn" };
            return StatusCode(409, new ErrorMsg { userMessage = "Invalid user id" });

        }
    }
}
