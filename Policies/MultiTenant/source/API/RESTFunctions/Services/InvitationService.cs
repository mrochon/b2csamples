using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using RESTFunctions.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace RESTFunctions.Services
{
    public class InvitationService
    {
        public InvitationService(IOptionsMonitor<InvitationTokenOptions> tokenOptions)
        {
            _tokenOptions = tokenOptions;
        }
        IOptionsMonitor<InvitationTokenOptions> _tokenOptions;
        private readonly Graph _graph;
        public async Task<string> GetInvitationUrl(ClaimsPrincipal inviter, InvitationDetails invite)
        {
            var tokenOptions = _tokenOptions.CurrentValue;
            var issuer = inviter.FindFirstValue("iss").Split('/');
            var domainName = issuer[2].Split('.')[0];
            var tenantId = issuer[3];
            var claims = new Dictionary<string, string>();
            if (invite.additionalClaims != null)
                foreach (var c in invite.additionalClaims)
                    claims.Add(c.Key, c.Value);
            claims.Add("appTenantId", inviter.FindFirstValue("appTenantId"));
            var hints = await GetTPDomain(invite.inviteEmail);
            var jwt = CreateJWTToken(invite.inviteEmail, domainName, domainName, _tokenOptions.CurrentValue.SigningKey, tokenOptions.ValidityHours, claims);
            var url = $"https://{domainName}.b2clogin.com/{tenantId}/{tokenOptions.Policy}/oauth2/v2.0/authorize?client_id={tokenOptions.clientId}{hints}&login_hint={invite.inviteEmail}&prompt=login&response_mode=query&nonce=defaultNonce&redirect_uri={tokenOptions.replyUrl}&scope=openid&response_type=id_token&client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion={jwt}";
            return url;
        }
        private string CreateJWTToken(string email, string issuer, string audience, string signingKey, int validityHours, IDictionary<string, string> userClaims = null)
        {
            IList<System.Security.Claims.Claim> claims = new List<System.Security.Claims.Claim>();
            claims.Add(new System.Security.Claims.Claim("email", email));
            if (userClaims != null)
            {
                foreach (var c in userClaims)
                {
                    claims.Add(new System.Security.Claims.Claim(c.Key, c.Value));
                }
            }
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
            var cred = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256Signature);
            var token = new JwtSecurityToken(
                issuer,
                audience, 
                claims, 
                DateTime.Now, 
                DateTime.Now.AddHours(validityHours), 
                cred);
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwt = jwtHandler.WriteToken(token);
            return jwt;
        }
        private static string[] _IdPDomains = { "gmail.com" };
        internal async Task<string> GetTPDomain(string email)
        {
            var http = new HttpClient();
            var domain = email.Split('@')[1];
            if (_IdPDomains.Contains(domain))
                return $"&domain_hint={domain}";
            try
            {
                var json = await http.GetStringAsync($"https://login.microsoftonline.com/{domain}/v2.0/.well-known/openid-configuration");
                JsonDocument.Parse(json).RootElement.GetProperty("token_endpoint");
                return $"&domain_hint=aadOrganizations&subdomain={domain}";
            }
            catch
            {
            }
            return "";
        }
    }
}
