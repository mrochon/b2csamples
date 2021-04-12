using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace B2CRestApis.Services
{
    public class GraphClient
    {
        private readonly IConfidentialClientApplication _msal;
        public GraphClient(IConfidentialClientApplication msal)
        {
            _msal = msal;
        }
    }
}
