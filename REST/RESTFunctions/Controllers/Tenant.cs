using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
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
    public class Tenant : ControllerBase
    {
        public Tenant(Graph graph)
        {
            _graph = graph;
        }
        Graph _graph;

        [HttpGet]
        public async Task<IActionResult> Get(string id)
        {
            Guid guid;
            if (!Guid.TryParse(id, out guid))
                return BadRequest("Invalid user id");
            var http = await _graph.GetClientAsync();
            try
            {
                var json = await http.GetStringAsync($"{Graph.BaseUrl}groups/{id}");
                var result = JObject.Parse(json);
                return new JsonResult(new TenantDef()
                {
                    Name = result["displayName"].Value<string>(),
                    Description = result["description"].Value<string>()
                });
            } catch (HttpRequestException)
            {
                return NotFound();
            }
        }
        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TenantDef tenant)
        {
            /*
            var headers = base.Request.Headers;
            var certHeader = headers["X-ARR-ClientCert"];
            if (!String.IsNullOrEmpty(certHeader))
            {
                try
                {
                    byte[] clientCertBytes = Convert.FromBase64String(certHeader);
                    var certificate = new X509Certificate2(clientCertBytes);
                } catch(Exception ex)
                {

                }
             }
             */

            if ((string.IsNullOrEmpty(tenant.Name) || (string.IsNullOrEmpty(tenant.UserObjectId))))
                return BadRequest("Invalid parameters");

            var http = await _graph.GetClientAsync();
            try
            {
                await http.GetStringAsync($"{Graph.BaseUrl}users/{tenant.UserObjectId}");
            } catch (HttpRequestException ex)
            {
                return BadRequest("Unable to validate user id");
            }
            if ((tenant.Name.Length > 60) || !Regex.IsMatch(tenant.Name, "^[A-Za-z]\\w*$"))
                return BadRequest("Invalid tenant name");
            var resp = await http.GetAsync($"{Graph.BaseUrl}groups?$filter=(displayName eq '{tenant.Name}')");
            if (!resp.IsSuccessStatusCode)
                return BadRequest("Unable to validate tenant existence");
            var values = JObject.Parse(await resp.Content.ReadAsStringAsync())["value"].Value<JArray>();
            if (values.Count != 0)
                return BadRequest("Tenant already exists");
            var group = new
            {
                description = tenant.Description,
                mailNickname = tenant.Name,
                displayName = tenant.Name.ToUpper(),
                groupTypes = new string[] { },
                mailEnabled = false,
                securityEnabled = true,
            };
            // add user who created this group as both owner and member
            var jGroup = JObject.FromObject(group);
            var owners = new string[] { $"{Graph.BaseUrl}users/{tenant.UserObjectId}" };
            jGroup.Add("owners@odata.bind", JArray.FromObject(owners));
            jGroup.Add("members@odata.bind", JArray.FromObject(owners));
            //  https://docs.microsoft.com/en-us/graph/api/group-post-groups?view=graph-rest-1.0&tabs=http
            resp = await http.PostAsync(
                $"{Graph.BaseUrl}groups",
                new StringContent(jGroup.ToString(), System.Text.Encoding.UTF8, "application/json"));
            if (!resp.IsSuccessStatusCode)
                return BadRequest("Tenant creation failed");
            var json = await resp.Content.ReadAsStringAsync();
            var newGroup = JObject.Parse(json);
            var id = newGroup["id"].Value<string>();
            // add this group to the user's tenant collection
            return new OkObjectResult(new { Id = id });
        }

        [HttpGet("forUser")]
        public async Task<IActionResult> GetForUser(string userId)
        {
            //if ((string.IsNullOrEmpty(userId) || (string.IsNullOrEmpty(userId))))
            //if (userId == Guid.Empty)
            //    return BadRequest("Invalid parameters");

            var http = await _graph.GetClientAsync();
            // does the user exist?
            //Guid guid;
            //if (!Guid.TryParse(userId, out guid))
            //    return BadRequest("Invalid user id");
            try
            {
                var json = await http.GetStringAsync($"{Graph.BaseUrl}users/{userId}/memberOf");
                var groups = JObject.Parse(json)["value"].Value<JArray>();
                var membership = new List<Member>();
                foreach (var group in groups)
                {
                    var isGroup = group["@odata.type"].Value<string>() == "#microsoft.graph.group";
                    if (!isGroup) continue;
                    var id = group["id"].Value<string>();
                    json = await http.GetStringAsync($"{Graph.BaseUrl}groups/{id}/owners");
                    var values = JObject.Parse(json)["value"].Value<JArray>();
                    var admin = values.FirstOrDefault(u => u["id"].Value<string>() == userId);
                    membership.Add(new Member
                    {
                        GroupId = group["id"].Value<string>(),
                        IsAdmin = admin != null
                    });
                }
                return new JsonResult(membership);
            }
            catch (HttpRequestException ex)
            {
                return BadRequest("Unable to validate user id");
            }
        }

        [HttpGet("getUserRole")]
        public async Task<IActionResult> GetUserRoleByNameAsync(string tenantName, string userId)
        {
            var http = await _graph.GetClientAsync();
            try
            {
                string role = null;
                string tenantId = await GetTenantIdFromNameAsync(tenantName);
                if (!String.IsNullOrEmpty(tenantId))
                {
                    role = await GetUserRoleByIdAsync(tenantId, userId);
                }
                return new JsonResult(new { tenantId, role });
            }
            catch (HttpRequestException ex)
            {
                return BadRequest("Errors processing this request");
            }
        }

        private async Task<string> GetUserRoleByIdAsync(string tenantId, string userId)
        {
            string role = null;
            if (await IsMemberAsync(tenantId, userId, true))
                role = "admin";
            else if (await IsMemberAsync(tenantId, userId, false))
                role = "member";
            return role;
        }
        private async Task<bool> IsMemberAsync(string tenantId, string userId, bool asAdmin = false)
        {
            var http = await _graph.GetClientAsync();
            var membType = asAdmin ? "owners" : "members";
            var json = await http.GetStringAsync($"{Graph.BaseUrl}groups/{tenantId}/{membType}");
            var members = JObject.Parse(json)["value"].Value<JArray>();
            var member = members.FirstOrDefault(m => m["id"].Value<string>() == userId.ToString());
            return (member != null);
        }

        [HttpPut("add")]
        public async Task<IActionResult> AddMember([FromBody] string tenantName, string userId, bool isAdmin = false)
        {
            var tenantId = await GetTenantIdFromNameAsync(tenantName);
            var http = await _graph.GetClientAsync();
            var refs = new List<string>() { "members" };
            if (isAdmin) refs.Add("owners");
            foreach (var refType in refs)
            {
                if (await IsMemberAsync(tenantId, userId, refType == "admin")) // skip if user already in this role
                    continue;
                var resp = await http.PostAsync(
                    $"{Graph.BaseUrl}groups/{tenantId}/{refType}/$ref",
                    new StringContent(
                        $"{{\"@odata.id\": \"https://graph.microsoft.com/v1.0/directoryObjects/{userId}\"}}",
                        System.Text.Encoding.UTF8,
                        "application/json"));
                if (!resp.IsSuccessStatusCode)
                    return BadRequest("Add member failed");
            }
            return new JsonResult(new { tenantId, role = isAdmin ? "admin" : "member" });
        }

        [HttpGet("{tenantName}/invite")]
        public IActionResult GetInviteToken(string tenantName, string email)
        {
            const string issuer = "http://b2cmultitenant";
            const string audience = "https://login.microsoftonline.com/mrochonb2cprod.onmicrosoft.com";

            IList<System.Security.Claims.Claim> claims = new List<System.Security.Claims.Claim>();
            claims.Add(new System.Security.Claims.Claim("appTenantId", tenantName));
            claims.Add(new System.Security.Claims.Claim("email", email));

            var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes("secret"));
            var cred = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256Signature);
            var token = new JwtSecurityToken(issuer, audience, claims, DateTime.Now, DateTime.Now.AddDays(1), cred);
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwt = jwtHandler.WriteToken(token);

            return new ObjectResult(jwt);
        }
        private async Task<string> GetTenantIdFromNameAsync(string tenantName)
        {
            var http = await _graph.GetClientAsync();
            var json = await http.GetStringAsync($"{Graph.BaseUrl}groups?$filter=(mailNickName eq {tenantName.ToUpper()})");
            var tenants = JObject.Parse(json)["value"].Value<JArray>();
            string tenantId = null;
            if (tenants.Count == 1)
            {
                tenantId = tenants[0]["id"].Value<string>();
                return tenantId;
            }
            return null;
        }
    }

    public class TenantDef
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string UserObjectId { get; set; }
    }
    public class Member
    {
        public string GroupId;
        public bool IsAdmin;
    }
}
