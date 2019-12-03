using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B2CMultiTenant.Models
{
    public class Member
    {
        public string Id { get; set; }
        public string Roles { get; set; }
        public string DisplayName { get; set; }
    }
}
