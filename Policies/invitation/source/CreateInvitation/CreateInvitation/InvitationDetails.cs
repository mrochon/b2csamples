using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateInvitation
{
    public class InvitationDetails
    {
        public string inviteEmail { get; set; }
        public string domain_hint { get; set; }        
        public Dictionary<string, string> additionalClaims { get; set; }
    }
}
