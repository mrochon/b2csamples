using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B2CMultiTenant.Extensions
{
    public class TokenService
    {
        public TokenService(IOptionsMonitor<ConfidentialClientApplicationOptions> options, IHttpContextAccessor ctx, IConfiguration conf)
        {
            _options = options;
            _httpContext = ctx;
            _configuration = conf;
        }
        public IConfidentialClientApplication AuthApp
        {
            get
            {
                if (_authApp == null)
                {
                    var journeyId = _httpContext.HttpContext.User.GetJourneyId();
                    _authApp = ConfidentialClientApplicationBuilder
                        .Create(_options.CurrentValue.ClientId)
                        .WithClientSecret(_options.CurrentValue.ClientSecret)
                        //.WithClientSecret(_configuration["AzureAD:ClientSecret"])
                        .WithB2CAuthority($"https://b2cmultitenant.b2clogin.com/tfp/{_options.CurrentValue.TenantId}/b2c_1a_{journeyId}")
                        .WithRedirectUri($"http://localhost:62385/signin-{journeyId}")
                        .Build();
                    _authApp.UserTokenCache.SetBeforeAccess(BeforeAccessNotification);
                    _authApp.UserTokenCache.SetAfterAccess(AfterAccessNotification);
                }
                return _authApp;
            }
        }
        IConfiguration _configuration;
        IConfidentialClientApplication _authApp;
        public async Task<string> GetUserTokenAsync(string[] scopes)
        {
            var accts = await AuthApp.GetAccountsAsync().ConfigureAwait(false);
            var acct = accts.FirstOrDefault();
            var tokens = await (AuthApp.AcquireTokenSilent(scopes, acct)).ExecuteAsync().ConfigureAwait(false);
            return tokens.AccessToken;
        }

        IOptionsMonitor<ConfidentialClientApplicationOptions> _options;
        IHttpContextAccessor _httpContext;
        private static readonly object _lock = new object();

        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            lock (_lock)
            {
                byte[] cache;
                if (_httpContext.HttpContext.Session.TryGetValue(_httpContext.HttpContext.User.GetCacheId(), out cache))
                {
                    args.TokenCache.DeserializeMsalV3(cache, shouldClearExistingCache: true);
                }
            }
        }

        private void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                lock (_lock)
                {
                    var cache = args.TokenCache.SerializeMsalV3();
                    _httpContext.HttpContext.Session.Set(_httpContext.HttpContext.User.GetCacheId(), cache);
                }
            }
        }
    }
}
