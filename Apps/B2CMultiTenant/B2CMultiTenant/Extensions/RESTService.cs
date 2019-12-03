using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace B2CMultiTenant.Extensions
{
    public class RESTService
    {
        public RESTService(TokenService tokenService)
        {
            _tokenService = tokenService;
        }
        public static readonly string[] Scopes =
        {
            "https://b2cmultitenant.onmicrosoft.com/b2crestapi/Members.ReadAll"
        };
        public static readonly string Url = "http://localhost:57688";
        TokenService _tokenService;
        public async Task<HttpClient> GetClientAsync()
        {
            var client = new HttpClient();
            var token = await _tokenService.GetUserTokenAsync(Scopes);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return client;
        }
    }
}
