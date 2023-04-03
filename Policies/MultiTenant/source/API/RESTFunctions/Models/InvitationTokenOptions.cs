using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RESTFunctions.Models
{
    public class InvitationTokenOptions
    {
        public string Policy { get; set; }              // Name of IEF policy to invoke in the signing request
        public string SigningKey { get; set; }          // symmetric signing key (same must be stored in PolicKeys)
        public int ValidityHours { get; set; }          // How long must the invitation be valid for
        public bool IncludeDomainHints { get; set; }    // include domain_hint and AAD-specific hint if email corresponds to existing AD tenant
        public string ClientId { get; set; }            // clientId of a b2C app on whose behalf invitation journey should be executed
        public string ReplyUrl { get; set; }            // replyUrl to which the user should be redirected after redeeming the invitation (must be registered for the above app)
    }
}
