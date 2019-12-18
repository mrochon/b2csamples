using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace B2CMultiTenant.Models
{
    public class Invitee
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public string InvitationUrl { get; set; }
    }
}
