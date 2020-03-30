using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B2CMultiTenant.Models
{
    public class TenantDetails
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string LongName { get; set; }
        public bool IsAADTenant { get; set; }
        public string IdPDomainName { get; set; }
    }
}
