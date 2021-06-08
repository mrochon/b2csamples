using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace B2CRestApis.Models
{
    public class InvitationDetails
    {
        public string inviteEmail { get; set; }
        public string postRedeemAppId { get; set; }
        public string postRedeemUrl { get; set; }
        public Dictionary<string, string> additionalClaims { get; set; }
    }
}
