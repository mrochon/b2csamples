using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RESTFunctions.Models;
using RESTFunctions.Services;

namespace RESTFunctions.Controllers
{
    // This partial class defines methods invoked directly from B2C policies using a client certificate for authz
    [Route("[controller]")]
    [ApiController]
    public partial class Tenant : ControllerBase
    {
        private readonly ILogger<Tenant> _logger;
        public Tenant(GraphClient graph, ILogger<Tenant> logger, InvitationService inviter, GraphOpenExtensions ext)
        {
            _graph = graph;
            _logger = logger;
            _logger.LogInformation("Tenant ctor");
            _inviter = inviter;
            _ext = ext;
        }
        GraphClient _graph;
        InvitationService _inviter;
        GraphOpenExtensions _ext;
        
        // Used by IEF
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TenantDetails tenant)
        {
            _logger.LogTrace("Starting POST /tenant");
            if ((User == null) || (!User.IsInRole("ief"))) return new UnauthorizedObjectResult("Unauthorized");
            _logger.LogTrace("Authorized");
            if ((string.IsNullOrEmpty(tenant.name) || (string.IsNullOrEmpty(tenant.ownerId))))
                return BadRequest(new { userMessage = "Bad parameters", status = 409, version = 1.0 });
            tenant.name = tenant.name.ToUpper();
            try
            {
                await _graph.GetStringAsync($"users/{tenant.ownerId}");
            } catch (HttpRequestException ex)
            {
                return BadRequest(new { userMessage = "Unknown user", status = 409, version = 1.0 });
            }
            if ((tenant.name.Length > 60) || !Regex.IsMatch(tenant.name, "^[A-Za-z]\\w*$"))
                return BadRequest(new { userMessage = "Invalid tenant name", status = 409, version = 1.0 });
            var resp = await _graph.GetAsync($"groups?$filter=(displayName eq '{tenant.name}')");
            if (!resp.IsSuccessStatusCode)
                return BadRequest(new { userMessage = "Unable to validate tenant existence", status = 409, version = 1.0 });
            var values = JObject.Parse(await resp.Content.ReadAsStringAsync())["value"].Value<JArray>();
            if (values.Count != 0)
                return new ConflictObjectResult(new { userMessage = "Tenant already exists", status = 409, version = 1.0 });
            var group = new
            {
                description = tenant.description,
                mailNickname = tenant.name,
                displayName = tenant.name,
                groupTypes = new string[] { },
                mailEnabled = false,
                securityEnabled = true,
            };
            var jGroup = JObject.FromObject(group);
            var owners = new string[] { $"{_graph.BaseAddress}users/{tenant.ownerId}" };
            jGroup.Add("owners@odata.bind", JArray.FromObject(owners));
            //jGroup.Add("members@odata.bind", JArray.FromObject(owners));
            //  https://docs.microsoft.com/en-us/graph/api/group-post-groups?view=graph-rest-1.0&tabs=http
            resp = await _graph.PostAsync(
                "groups", new StringContent(jGroup.ToString(), System.Text.Encoding.UTF8, "application/json"));
            if (!resp.IsSuccessStatusCode)
            {
                var error = await resp.Content.ReadAsStringAsync();
                _logger.LogError($"Tenant {tenant.name} creation for user {tenant.ownerId} failed: {error}");
                return BadRequest("Tenant creation failed");
            }
            var json = await resp.Content.ReadAsStringAsync();
            var newGroup = JObject.Parse(json);
            var id = newGroup["id"].Value<string>();
            // Add extensions (open)
            tenant.id = id;
            tenant.allowSameIssuerMembers = (!String.IsNullOrEmpty(tenant.allowSameIssuerMembersString) && (String.Compare("allow", tenant.allowSameIssuerMembersString) == 0));
            if (!(await _ext.CreateAsync(tenant)))
                return BadRequest("Tenant extensions creation failed");
            // add this group to the user's tenant collection
            var allTenants = await GetTenantsForUserImpl(tenant.ownerId);
            _logger.LogInformation("Finishing Create tenant");
            return new OkObjectResult(new TenantUserResponse
            { 
                tenantId = tenant.id, 
                tenantName = tenant.name,
                roles = new string[] { "Tenant.admin", "Tenant.member" }, 
                allTenants = allTenants.Select(t => t.tenantName)
            });
        }
        [HttpGet("forUser")]
        public async Task<IActionResult> GetForUser(string userId)
        {
            using (_logger.BeginScope("forUser"))
            {
                if ((User == null) || (!User.IsInRole("ief"))) return new UnauthorizedObjectResult("Unauthorized");
                _logger.LogInformation("Authorized");
                try
                {
                    var json = await _graph.GetStringAsync($"users/{userId}/memberOf");
                    var groups = JObject.Parse(json)["value"].Value<JArray>();
                    var membership = new
                    {
                        tenantIds = new List<string>(),
                        tenants = new List<string>(),
                        roles = new List<string>()
                    };
                    _logger.LogInformation("Processing groups");
                    foreach (var group in groups)
                    {
                        var isGroup = group["@odata.type"].Value<string>() == "#microsoft.graph.group";
                        if (!isGroup) continue;
                        var id = group["id"].Value<string>();
                        json = await _graph.GetStringAsync($"groups/{id}/owners");
                        var values = JObject.Parse(json)["value"].Value<JArray>();
                        var admin = values.FirstOrDefault(u => u["id"].Value<string>() == userId);
                        membership.tenantIds.Add(group["id"].Value<string>());
                        membership.tenants.Add(group["displayName"].Value<string>());
                        membership.roles.Add(admin != null ? "Tenant.admin" : "Tenant.member");
                    }
                    if (membership.tenantIds.Count == 0)
                        return new JsonResult(new { });
                    return new JsonResult(membership);
                }
                catch (HttpRequestException ex)
                {
                    return BadRequest("Unable to validate user id");
                }
            }
        }
        [HttpGet("getUserRoles")]
        public async Task<IActionResult> GetUserRolesByNameAsync(string tenantName, string userId)
        {
            if ((User == null) || (!User.IsInRole("ief"))) return new UnauthorizedObjectResult("Unauthorized");
            try
            {
                IEnumerable<string> roles = null;
                string tenantId = await GetTenantIdFromNameAsync(tenantName);
                if (!String.IsNullOrEmpty(tenantId))
                {
                    roles = await GetUserRolesByIdAsync(tenantId, userId);
                }
                return new JsonResult(new { tenantName, roles });
            }
            catch (HttpRequestException ex)
            {
                return BadRequest("Errors processing this request");
            }
        }
        private async Task<IEnumerable<string>> GetUserRolesByIdAsync(string tenantId, string userId)
        {
            List<string> roles = new List<string>();
            if (await IsMemberAsync(tenantId, userId, true))
                roles.Add("Tenant.admin");
            else if (await IsMemberAsync(tenantId, userId, false))
                roles.Add("Tenant.member");
            else
                roles = null;
            return roles;
        }
        private async Task<bool> IsMemberAsync(string tenantId, string userId, bool asAdmin = false)
        {
            var membType = asAdmin ? "owners" : "members";
            var json = await _graph.GetStringAsync($"groups/{tenantId}/{membType}");
            var members = JObject.Parse(json)["value"].Value<JArray>();
            var member = members.FirstOrDefault(m => m["id"].Value<string>() == userId.ToString());
            return (member != null);
        }

        // Used by IEF
        // add or confirm user is member, return roles
        [HttpPost("member")]
        public async Task<IActionResult> Member([FromBody] TenantIdMember memb)
        {
            _logger.LogTrace("Member: {0}", memb.tenantId);
            if ((User == null) || (!User.IsInRole("ief"))) return new UnauthorizedObjectResult("Unauthorized");
            var tenantId = memb.tenantId;
            _logger.LogTrace("Tenant id: {0}", tenantId);
            if (String.IsNullOrEmpty(tenantId))
                return new NotFoundObjectResult(new { userMessage = "Tenant does not exist", status = 404, version = 1.0 });
            string appTenantName;
            try
            {
                var json = await _graph.GetStringAsync($"groups/{tenantId}");
                appTenantName = JObject.Parse(json).Value<string>("displayName");
            } catch(Exception)
            {
                return new NotFoundObjectResult(new { userMessage = "Tenant does not exist", status = 404, version = 1.0 });
            }
            if (await IsMemberAsync(tenantId, memb.userId, true)) // skip already an admin
            {
                return new JsonResult(new { tenantId, tenantName = appTenantName, roles = new string[] { "Tenant.admin", "Tenant.member" } });
            }
            else if (await IsMemberAsync(tenantId, memb.userId, false))
            {
                return new JsonResult(new { tenantId, tenantName = appTenantName, roles = new string[] { "Tenant.member" } });
            }
            else
            {
                var segment = (!String.IsNullOrEmpty(memb.isAdmin) && (memb.isAdmin.ToLower() == "true")) ? "owners" : "members";
                var resp = await _graph.PostAsync(
                    $"groups/{tenantId}/{segment}/$ref",
                    new StringContent(
                        $"{{\"@odata.id\": \"https://graph.microsoft.com/v1.0/directoryObjects/{memb.userId}\"}}",
                        System.Text.Encoding.UTF8,
                        "application/json"));
                if (!resp.IsSuccessStatusCode)
                {
                    var error = await resp.Content.ReadAsStringAsync();
                    _logger.LogError($"Add member {memb.userId} to tenant {tenantId}: {error}");
                    return BadRequest("Add owener/member failed");
                }
                return new JsonResult(new { tenantId, tenantName = appTenantName, roles = new string[] { "Tenant.member" }, isNewMember = true });
            }
        }
        // Used by IEF
        [HttpGet("GetTenantsForUser")]
        public async Task<IActionResult> GetTenantsForUser([FromQuery] string userId, string tenantName, string identityProvider, string directoryId)
        {
            _logger.LogInformation($"GetTenantsForUser: User id:{userId}, tenantName: {tenantName}");
            if ((User == null) || (!User.IsInRole("ief"))) return new UnauthorizedObjectResult("Unauthorized");
            if (String.IsNullOrEmpty(userId))
            {
                _logger.LogError("GetTenantsForUser called with empty userId");
                return new OkResult();
            }

            Member tenant = null;
            IEnumerable<Member> userTenants = null;
            tenantName = tenantName?.ToUpper();

            userTenants = await GetTenantsForUserImpl(userId);
            tenant = userTenants?.FirstOrDefault(t => String.Compare(t.tenantName, tenantName, true) == 0);

            if ((tenant == null) && !String.IsNullOrEmpty(tenantName) && String.Equals("aadOrganizations", identityProvider)) // perhaps this tenant allows users from same directory as creator
            {
                var tenantId = await GetTenantIdFromNameAsync(tenantName);
                if (!String.IsNullOrEmpty(tenantId))
                {
                    var t = await _ext.GetAsync(new TenantDetails() { id = tenantId });
                    if (String.Equals(directoryId, t.directoryId) && t.allowSameIssuerMembers)
                    {
                        var segment = "members";
                        var resp = await _graph.PostAsync(
                            $"groups/{tenantId}/{segment}/$ref",
                            new StringContent(
                                $"{{\"@odata.id\": \"https://graph.microsoft.com/v1.0/directoryObjects/{userId}\"}}",
                                System.Text.Encoding.UTF8,
                                "application/json"));
                        if (!resp.IsSuccessStatusCode)
                        {
                            _logger.LogError($"GetTenantsForUser failed to add AAD user {userId} to tenant {tenantName}");
                            return new OkResult();
                        }
                        tenant = new Member() { tenantId = tenantId, tenantName = tenantName, roles = new List<string> { "member" }, userId = userId };
                        if (userTenants == null)
                            userTenants = new List<Member>() { tenant };
                        else
                        {
                            var list = userTenants.ToList();
                            list.Add(tenant);
                            userTenants = list;
                        }
                        // This is a new AAD user so will not have any roles in B2C yet
                        return new JsonResult(new TenantUserResponse
                        {
                            tenantId = tenant.tenantId,
                            tenantName = tenant.tenantName,
                            requireMFA = t.requireMFA,
                            roles = tenant.roles, // .Aggregate((a, s) => $"{a},{s}"),
                            allTenants = userTenants.Select(t => t.tenantName),  // .Aggregate((a, s) => $"{a},{s}")
                            newUser = false
                        });
                    }
                }
            }
            if(tenant == null) // if still no tenant
                tenant = userTenants?.FirstOrDefault();

            if (tenant != null)
            {
                var t = await _ext.GetAsync(new TenantDetails() { id = tenant.tenantId });
                return new JsonResult(new TenantUserResponse
                {
                    tenantId = tenant.tenantId,
                    tenantName = tenant.tenantName,
                    requireMFA = t.requireMFA,
                    roles = tenant.roles, // .Aggregate((a, s) => $"{a},{s}"),
                    allTenants = userTenants.Select(t => t.tenantName),  // .Aggregate((a, s) => $"{a},{s}")
                    newUser = false
                });
            }
            _logger.LogWarning($"GetTenantsForUser: failed attempt to by {userId} to join {tenantName}");
            return new OkResult(); // empty response
        }
        [HttpPost("GetAppRoles")]
        public async Task<IActionResult> GetAppRoles([FromBody] UserRoles userRoles)
        {
            IEnumerable<string> result = null;
            try
            {
                result = await _graph.GetAppRoles(userRoles.appId, userRoles.userObjectId);
                if (userRoles.roles != null)
                    result = result.Concat(userRoles.roles);
            } catch(Exception ex)
            {
                _logger.LogError($"GetAppRoles: {ex.Message}");
            }
            return new JsonResult(result);
        }
        [HttpGet("IsSignupAllowed")]
        public async Task<IActionResult> IsSignupAllowed([FromQuery] string appTenantName, string identityProvider = "", string directoryId = "")
        {
            _logger.LogInformation("IsSignupAllowed");
            _logger.LogInformation($"appTenantname is null?{String.IsNullOrEmpty(appTenantName)}");
            if ((User == null) || (!User.IsInRole("ief"))) return new UnauthorizedObjectResult("Unauthorized");
            bool isSignupAllowed = false;
            if (!String.IsNullOrEmpty(appTenantName))
            {
                _logger.LogInformation($"Tenant: {appTenantName}, identityprovider: {identityProvider}, directoryId: {directoryId}");
                var id = await GetTenantIdFromNameAsync(appTenantName);
                if (!String.IsNullOrEmpty(id))
                {
                    var t = await _ext.GetAsync(new TenantDetails() { id = id });
                    _logger.LogInformation($"Found: {t.allowSameIssuerMembers}, {t.identityProvider}, {t.directoryId}");
                    isSignupAllowed = t.allowSameIssuerMembers 
                        && (t.identityProvider == identityProvider)
                        && (t.directoryId == directoryId);
                }
            }
            _logger.LogInformation($"Result: {isSignupAllowed}");
            return new JsonResult(new { isSignupAllowed });
        }
        private async Task<string> GetTenantIdFromNameAsync(string tenantName)
        {
            var json = await _graph.GetStringAsync($"groups?$filter=(mailNickName eq '{tenantName.ToUpper()}')");
            var tenants = JObject.Parse(json)["value"].Value<JArray>();
            string tenantId = null;
            if (tenants.Count == 1)
            {
                tenantId = tenants[0]["id"].Value<string>();
                return tenantId;
            }
            return null;
        }
        private async Task<IEnumerable<Member>> GetTenantsForUserImpl(string userId)
        {
            var result = new List<Member>();
            try
            {
                foreach (var role in new string[] { "ownedObjects", "memberOf" })
                {
                    var json = await _graph.GetStringAsync($"users/{userId}/{role}");
                    var groups = JObject.Parse(json)["value"].Value<JArray>();
                    foreach (var group in groups)
                    {
                        var isGroup = group["@odata.type"].Value<string>() == "#microsoft.graph.group";
                        if (!isGroup) continue;
                        var tenantId = group["id"].Value<string>();
                        var currTenant = result.FirstOrDefault(m => m.tenantId == tenantId);
                        if (currTenant != null)
                            currTenant.roles.Add(role == "ownedObjects" ? "Tenant.admin" : "Tenant.member");
                        else
                            result.Add(new Member()
                            {
                                tenantId = group["id"].Value<string>(),
                                tenantName = group["displayName"].Value<string>(),
                                roles = new List<string>() { role == "ownedObjects" ? "Tenant.admin" : "Tenant.member" },
                                userId = userId
                            });
                    }
                }
                return result;
            }
            catch (HttpRequestException ex)
            {
                return null;
            }
        }
    }

   /* public class TenantDef
    {
        public string name { get; set; }
        public string description { get; set; }
        public string ownerId { get; set; }
        public bool requireMFA { get; set; }
        public string identityProvider { get; set; }
        public string tenantId { get; set; }
    } */
    public class TenantMember
    {
        public string tenantName { get; set; }
        public string userId { get; set; }
        public string identityProvider { get; set; }
        public string directoryId { get; set; }
    }
    public class TenantIdMember
    {
        public string tenantId { get; set; }
        public string userId { get; set; }
        public string isAdmin { get; set; }  // string of boolean
    }
    public class Member
    {
        public string tenantId { get; set; }
        public string tenantName { get; set; }
        public string userId { get; set; }
        public List<string> roles { get; set; }
        public string name { get; set; }
        public string email { get; set; }
    }
    public class UserRoles
    {
        public string appId { get; set; }
        public string userObjectId { get; set; }
        public IEnumerable<string> roles { get; set; }
    }
}
