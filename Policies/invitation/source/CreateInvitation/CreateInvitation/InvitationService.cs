using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace CreateInvitation
{
    public class InvitationService
    {
        public InvitationService(InvitationTokenOptions tokenOptions)
        {
            _options = tokenOptions;
        }
        InvitationTokenOptions _options;
        public string GetInvitationUrl(InvitationDetails invite)
        {
            IList<System.Security.Claims.Claim> claims = new List<System.Security.Claims.Claim>();
            claims.Add(new System.Security.Claims.Claim("email", invite.inviteEmail));
            if (invite.additionalClaims != null)
            {
                foreach (var c in invite.additionalClaims)
                {
                    claims.Add(new System.Security.Claims.Claim(c.Key, c.Value));
                }
            }
            var replyUrl = System.Web.HttpUtility.UrlEncode(_options.RedeemReplyUrl);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
            var cred = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256Signature);
            var token = new JwtSecurityToken(
                issuer:_options.Issuer,
                audience:_options.Audience,
                claims,
                DateTime.Now,
                DateTime.Now.AddMinutes(_options.ValidityMinutes),
                cred);
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwt = jwtHandler.WriteToken(token); var url = $"https://{_options.TenantName}.b2clogin.com/{_options.TenantName}.onmicrosoft.com/{_options.Policy}/oauth2/v2.0/authorize?client_id={_options.ClientId}&domain_hint={invite.domain_hint}&login_hint={invite.inviteEmail}&response_mode=form_post&nonce=defaultNonce&redirect_uri={replyUrl}&scope=openid&response_type=id_token&client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion={jwt}";
            return url;
        }
    }
}