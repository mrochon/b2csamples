using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace B2CMultiTenant.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetCacheId(this ClaimsPrincipal user)
        {
            var objectId = user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
            var tenantId = user.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
            var key = $"{objectId}-{user.GetJourneyId()}.{tenantId}";
            return key;
        }
        public static string GetJourneyId(this ClaimsPrincipal user)
        {
            return user.FindFirst("http://schemas.microsoft.com/claims/authnclassreference").Value.Split('_')[2];
        }
    }
}
