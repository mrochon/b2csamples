using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using B2CRestApis.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using B2CRestApis.Models;
using Microsoft.Identity.Web.Resource;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace B2CRestApis.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class User : ControllerBase
    {
        private readonly ILogger<User> _logger;
        private readonly IHttpClientFactory _clientFactory;
        private readonly InvitationService _inviter;

        public User(
            IHttpClientFactory clientFactory, 
            ILogger<User> logger,
            InvitationService inviter)
        {
            _logger = logger;
            _clientFactory = clientFactory;
            _inviter = inviter;
        }

        [HttpGet("roles")]
        public async Task<object> Roles([FromQuery] string userObjectId, string appId)
        {
            _logger.LogInformation($"user/roles?userObjectId={userObjectId}&appId={appId}");
            using (var http = _clientFactory.CreateClient("B2C"))
            {
                try
                {
                    // Cache this!!  https://docs.microsoft.com/en-us/aspnet/core/performance/caching/memory?view=aspnetcore-5.0#:~:text=Cache%20in-memory%20in%20ASP.NET%20Core%201%20Caching%20basics.,8%20Background%20cache%20update.%20...%209%20Additional%20resources
                    var json = await http.GetStringAsync($"{http.BaseAddress}applications?$filter=(appId eq '{appId}')&$select=appRoles");
                    var appRoles = JObject.Parse(json)["value"].First()["appRoles"]
                        .Where(role => role["isEnabled"].Value<bool>())
                        .Select(role => new { id = role["id"].Value<string>(), value = role["value"].Value<string>() });
                    _logger.LogInformation($"Retrieved {appRoles.Count()} roles");
                    json = await http.GetStringAsync($"{http.BaseAddress}users/{userObjectId}/appRoleAssignments");
                    var roleAssignments = (JArray)JObject.Parse(json)["value"];
                    var roles = roleAssignments
                        .Join(appRoles, ra => ((JObject)ra)["appRoleId"].Value<string>(), role => role.id, (ra, role) => role.value).ToList();
                    return new { roles };
                }
                catch (Exception ex)
                {
                    _logger.LogError($"user/roles exception: {ex.Message}");
                }
            }
            return null;
        }
        [HttpGet("find")]
        public async Task<object> Find([FromQuery] string id, string tenantName)
        {
            _logger.LogDebug($"Starting Find({id}");
            using (var http = _clientFactory.CreateClient("B2C"))
            {
                try
                {
                    var url = $"https://graph.microsoft.com/v1.0/users?$filter=identities/any(c:c/issuerAssignedId eq '{id}' and c/issuer eq '{tenantName}')&$select=id";
                    var json = await http.GetStringAsync(url);
                    var objectId = JObject.Parse(json)["value"].First()["id"].Value<string>();
                    _logger.LogDebug($"Find({id}) returning {objectId}");
                    return objectId;
                } catch
                {
                    _logger.LogDebug($"Starting Find({id} returning 'not found'");
                    return StatusCode(404, new ErrorMsg { status = HttpStatusCode.Unauthorized, userMessage = "User not found" });
                }
            }
        }
        static readonly string[] scopeRequiredByApi = new string[] { "User.Invite" };
        [HttpPost("oauth2/invite")]
        [Authorize(AuthenticationSchemes=JwtBearerDefaults.AuthenticationScheme)]
        public string Invite([FromBody] InvitationDetails invite)
        {
            HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
            return _inviter.GetInvitationUrl(User, invite);
        }
    }
}
