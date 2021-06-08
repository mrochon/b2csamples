using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B2CRestApis.Models
{
    public class InvitationTokenOptions
    {
        public string Policy { get; set; }
        public string SigningKey { get; set; }
        public int ValidityMinutes { get; set; }
        public string RedeemReplyUrl { get; set; }
    }
}
