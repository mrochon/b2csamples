using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RESTFunctions.Models
{
    public class TenantDetails
    {
        public string Name { get; set; }
        public string LongName { get; set; }
        public bool IsAADTenant { get; set; }
        public string IdPDomainName { get; set; }
    }
}
