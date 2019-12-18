using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B2CMultiTenant.Models
{
    public class InvitationTokenOptions
    {
        public string Domain { get; set; }
        public string InvitationPolicy { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string SigningKey { get; set; }
        public int ValidityHours { get; set; }
        public string RedirectPath { get; set; }
        public string ClientId { get; set; }

    }
}
