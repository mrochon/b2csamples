using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMigration
{
    internal class MailOptions
    {
        public string? SenderEmail { get; set; }
        public string? SenderName { get; set; }
        public string? ApiKey { get; set; }
        public string? B2CAppId { get; set; }
    }
}
