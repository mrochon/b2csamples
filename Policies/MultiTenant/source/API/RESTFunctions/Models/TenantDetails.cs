using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RESTFunctions.Models
{
    public class TenantDetails
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string ownerId { get; set; }
        public bool requireMFA { get; set; }
        public string identityProvider { get; set; }
        public string directoryId { get; set; }
        public bool allowSameIssuerMembers { get; set; }
        public string allowSameIssuerMembersString { get; set; }
    }
}
