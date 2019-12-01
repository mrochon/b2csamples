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
        public TokenService(IOptionsMonitor<OpenIdConnectOptions> options, IHttpContextAccessor ctx, IConfiguration conf)
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
                    _authApp = ConfidentialClientApplicationBuilder
                        .Create(_options.CurrentValue.ClientId)
                        .WithClientSecret(_configuration["AzureAD:ClientSecret"])
                        .WithB2CAuthority("https://b2cmultitenant.b2clogin.com/tfp/b2cmultitenant.onmicrosoft.com/b2c_1a_mtsusi")
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
            var acctId = _httpContext.HttpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/objectid").Value;
            var acct = await AuthApp.GetAccountAsync(acctId);
            var tokens = await (AuthApp.AcquireTokenSilent(scopes, acct)).ExecuteAsync();
            return tokens.AccessToken;
        }

        private static readonly string SessionPrefix = "__MSAL";
        IOptionsMonitor<OpenIdConnectOptions> _options;
        IHttpContextAccessor _httpContext;
        private static readonly object _lock = new object();

        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            lock (_lock)
            {
                byte[] cache;
                if (_httpContext.HttpContext.Session.TryGetValue($"{SessionPrefix}.{args.Account.HomeAccountId.ObjectId}", out cache))
                {
                    args.TokenCache.DeserializeMsalV3(cache);
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
                    _httpContext.HttpContext.Session.Set($"{SessionPrefix}.{args.Account.HomeAccountId.ObjectId}", cache);
                }
            }
        }
    }
}
