using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace B2CMultiTenant.Models
{
    public class TenantDetails
    {
        public string Id { get; set; }
        [Required(ErrorMessage = "Please enter name"), MaxLength(30)]
        public string Name { get; set; }
        [Required(ErrorMessage = "Please enter description"), MaxLength(30)]
        [Display(Name = "Long name")]
        public string LongName { get; set; }
        public bool IsAADTenant { get; set; }
        public string IdPDomainName { get; set; }
    }
}
