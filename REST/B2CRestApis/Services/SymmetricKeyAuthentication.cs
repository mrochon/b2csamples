using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B2CRestApis.Services
{
    public class SymmetricKeyAuthentication : IBasicAuthenticationService
    {
        public SymmetricKeyAuthentication(IOptionsMonitor<BasicAuthOptions> opts)
        {
            _opts = opts;
        }
        IOptionsMonitor<BasicAuthOptions> _opts;
        public Task<bool> IsValidUserAsync(string user, string password)
        {
            return Task.FromResult((string.Compare(user, _opts.CurrentValue.userId) == 0) && (string.Compare(password, _opts.CurrentValue.password) == 0));
        }
    }

    public class BasicAuthOptions
    {
        public string userId { get; set; }
        public string password { get; set; }
    }
}
