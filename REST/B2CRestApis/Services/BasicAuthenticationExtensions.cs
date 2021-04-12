using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace B2CRestApis.Services
{
    public interface IBasicAuthenticationService
    {
        Task<bool> IsValidUserAsync(string user, string password);
    }
    public static partial class BasicAuthenticationExtensions
    {
        public static AuthenticationBuilder AddBasicAuthentication(
            this AuthenticationBuilder builder, 
            string authenticationScheme = BasicAuthenticationDefaults.AuthenticationScheme, 
            Action<BasicAuthenticationOptions> configureOptions = null
        )
        {
            if (configureOptions == null)
                configureOptions = _ => new BasicAuthenticationOptions();
            return builder.AddScheme<BasicAuthenticationOptions, BasicAuthenticationHandler>(
                authenticationScheme, configureOptions);
        }
    }
    public static class BasicAuthenticationDefaults
    {
        public const string AuthenticationScheme = "BasicAuthentication";
        public const string ConfigSection = "BasicAuth";
    }

    public class BasicAuthenticationOptions: AuthenticationSchemeOptions
    {
        public BasicAuthenticationOptions()
        {
            Realm = "BasicAuth";
        }
        public string Realm { get; set; }
    }

    public class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
    {
        private const string AuthorizationHeaderName = "Authorization";
        private const string BasicSchemeName = "Basic";
        private readonly IBasicAuthenticationService _authenticationService;

        public BasicAuthenticationHandler(
            IOptionsMonitor<BasicAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IBasicAuthenticationService authenticationService)
            : base(options, logger, encoder, clock)
        {
            _authenticationService = authenticationService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey(AuthorizationHeaderName))
            {
                //Authorization header not in request
                return AuthenticateResult.NoResult();
            }
            if (!AuthenticationHeaderValue.TryParse(Request.Headers[AuthorizationHeaderName], out AuthenticationHeaderValue headerValue))
            {
                //Invalid Authorization header
                return AuthenticateResult.NoResult();
            }
            if (!BasicSchemeName.Equals(headerValue.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                //Not Basic authentication header
                return AuthenticateResult.NoResult();
            }

            byte[] headerValueBytes = Convert.FromBase64String(headerValue.Parameter);
            string userAndPassword = Encoding.UTF8.GetString(headerValueBytes);
            string[] parts = userAndPassword.Split(':');
            if (parts.Length != 2)
            {
                return AuthenticateResult.Fail("Invalid Basic authentication header");
            }
            string user = parts[0];
            string password = parts[1];

            bool isValidUser = await _authenticationService.IsValidUserAsync(user, password);

            if (!isValidUser)
            {
                return AuthenticateResult.Fail("Invalid username or password");
            }
            var claims = new[] { new Claim(ClaimTypes.Name, user) };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.Headers["WWW-Authenticate"] = $"Basic realm=\"{Options.Realm}\", charset=\"UTF-8\"";
            await base.HandleChallengeAsync(properties);
        }
    }
}
