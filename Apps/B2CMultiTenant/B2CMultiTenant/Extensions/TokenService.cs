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
                        //.WithB2CAuthority("https://fabrikamb2c.b2clogin.com/tfp/{tenant}/{PolicySignInSignUp}")
                        .WithB2CAuthority("https://b2cmultitenant.b2clogin.com/tfp/b2cmultitenant.onmicrosoft.com/b2c_1a_mtsusi")
                        //.WithB2CAuthority("https://b2cmultitenant.b2clogin.com/b2cmultitenant.onmicrosoft.com/b2c_1a_mtsusi/v2.0") // errors: must have 3 segments, incl. tfp or invalid instance
                        .WithRedirectUri("http://localhost:62385/signin-mtsusi")
                        .Build();
                    //_authApp.UserTokenCache.SetBeforeAccess(BeforeAccessNotification);
                    //_authApp.UserTokenCache.SetAfterAccess(AfterAccessNotification);
                }
                return _authApp;
            }
        }
        IConfiguration _configuration;
        IConfidentialClientApplication _authApp;
        public async Task<string> GetUserTokenAsync(string[] scopes)
        {
            var accts = await _authApp.GetAccountsAsync().ConfigureAwait(false);
            var acct = accts.FirstOrDefault();
            var tokens = await (_authApp.AcquireTokenSilent(scopes, acct)).ExecuteAsync().ConfigureAwait(false);
            return tokens.AccessToken;
        }

        IOptionsMonitor<OpenIdConnectOptions> _options;
        IHttpContextAccessor _httpContext;
        private static readonly object _lock = new object();

        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            lock (_lock)
            {
                byte[] cache;
                string key;
                if (args.Account == null)
                {
                    var objectId = _httpContext.HttpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                    var tenantId = _httpContext.HttpContext.User.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
                    key = $"{objectId}-b2c_1a_mtsusi.{tenantId}";
                }
                else
                    key = args.Account.HomeAccountId.Identifier;
                if (_httpContext.HttpContext.Session.TryGetValue(key, out cache))
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
                    //var objectId = _httpContext.HttpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                    //var tenantId = _httpContext.HttpContext.User.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
                    //var key = $"{objectId}-b2c_1a_mtsusi.{tenantId}";
                    var key = args.Account.HomeAccountId.Identifier;
                    _httpContext.HttpContext.Session.Set(key, cache);
                }
            }
        }
    }
}
