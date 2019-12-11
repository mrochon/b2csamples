using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B2CMultiTenant.Models
{
    public class Invitee
    {
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public string InvitationUrl { get; set; }
    }
}
