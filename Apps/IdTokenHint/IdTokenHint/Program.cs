using Microsoft.Identity.Client;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace IdTokenHint
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var p = new Program();
            var id = p.SignInAsync().Result;
            id = p.Refresh().Result;
            id = p.EditProfileAsync(id).Result;
        }
        private static string Tenant = "mrochonb2cprod";
        private static string AuthBase = $"https://{Tenant}.b2clogin.com/tfp/{Tenant}.onmicrosoft.com/";
        private static string ClientId = "75bb114c-bb4d-4134-8fc6-1f3aa84b0759";
        private static IPublicClientApplication Client;

        private async Task<string> SignInAsync()
        {
            Client = PublicClientApplicationBuilder
                .Create(ClientId)
                .WithB2CAuthority($"{AuthBase}B2C_1A_IDHINTsignup_signin")
                .WithRedirectUri("http://localhost")
                .Build();
            var tokens = await Client.AcquireTokenInteractive(new string[] { "offline_access" }).ExecuteAsync();
            return tokens.IdToken;
        }
        private async Task<string> Refresh()
        {
            var accounts = await Client.GetAccountsAsync();
            var account = accounts.First();
            var tokens = await Client.AcquireTokenSilent(new string[] { "openid" }, account).ExecuteAsync();
            return tokens.IdToken;
        }
        private async Task<string> EditProfileAsync(string id_token)
        {
            var client = PublicClientApplicationBuilder
                .Create(ClientId)
                .WithB2CAuthority($"{AuthBase}B2C_1A_IDHINTProfileEdit")
                .WithRedirectUri("http://localhost")
                .WithExtraQueryParameters($"id_token_hint={id_token}")
                .Build();
            var tokens = await client.AcquireTokenInteractive(new string[] { }).ExecuteAsync();
            return tokens.IdToken;
        }
    }
}
