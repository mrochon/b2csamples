using B2CMultiTenant.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace B2CMultiTenant.Extensions
{
    public class InvitationService
    {
        public InvitationService(
            IHttpContextAccessor http,
            IOptionsMonitor<InvitationTokenOptions> tokenOptions, 
            IOptionsMonitor<ConfidentialClientApplicationOptions> clientOptions)
        {
            _httpContext = http;
            _tokenOptions = tokenOptions;
            _clientOptions = clientOptions;
        }
        public string GetInvitationUrl(string email, IDictionary<string,string> userClaims = null)
        {
            var tokenOptions = _tokenOptions.CurrentValue;
            var clientOptions = _clientOptions.CurrentValue;
            var domainId = tokenOptions.Domain.Split('.')[0];
            if (string.IsNullOrEmpty(clientOptions.TenantId))
                clientOptions.TenantId = tokenOptions.Domain;
            var req = _httpContext.HttpContext.Request;
            var replyUrl = $"{req.Scheme}://{req.Host}{_tokenOptions.CurrentValue.RedirectPath}";
            var jwt = CreateJWTToken(email, userClaims);
            var url = $"https://{domainId}.b2clogin.com/{clientOptions.TenantId}/{tokenOptions.InvitationPolicy}/oauth2/v2.0/authorize?client_id={tokenOptions.ClientId}&login_hint={email}&response_mode=form_post&nonce=defaultNonce&redirect_uri={HttpUtility.UrlEncode(replyUrl)}&scope=openid&response_type=id_token&client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion={jwt}";
            return url;
        }
        IHttpContextAccessor _httpContext;
        IOptionsMonitor<InvitationTokenOptions> _tokenOptions;
        IOptionsMonitor<ConfidentialClientApplicationOptions> _clientOptions;
        public string CreateJWTToken(string email, IDictionary<string, string> userClaims = null)
        {
            IList<System.Security.Claims.Claim> claims = new List<System.Security.Claims.Claim>();
            claims.Add(new System.Security.Claims.Claim("email", email));
            foreach(var c in userClaims)
            {
                claims.Add(new System.Security.Claims.Claim(c.Key, c.Value));
            }
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenOptions.CurrentValue.SigningKey));
            var cred = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256Signature);
            var token = new JwtSecurityToken(
                _tokenOptions.CurrentValue.Issuer,
                _tokenOptions.CurrentValue.Audience, 
                claims, 
                DateTime.Now, 
                DateTime.Now.AddHours(_tokenOptions.CurrentValue.ValidityHours), 
                cred);
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwt = jwtHandler.WriteToken(token);

            return jwt;
        }
    }
}
