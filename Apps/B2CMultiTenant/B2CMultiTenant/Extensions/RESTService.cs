using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace B2CMultiTenant.Extensions
{
    public class RESTService
    {
        public RESTService(TokenService tokenService, IConfiguration conf)
        {
            _tokenService = tokenService;
            _conf = conf;
            Url = conf["RESTUrl"];
        }
        public static readonly string[] Scopes =
        {
            "https://b2cmultitenant.onmicrosoft.com/b2crestapi/Members.ReadAll",
            "offline_access"
        };
        //public static readonly string Url = "http://localhost:57688";
        public static string Url
        {
            get;
            private set;
        }
        TokenService _tokenService;
        IConfiguration _conf;
        public async Task<HttpClient> GetClientAsync()
        {
            var client = new HttpClient();
            var token = await _tokenService.GetUserTokenAsync(Scopes);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return client;
        }
    }
}
