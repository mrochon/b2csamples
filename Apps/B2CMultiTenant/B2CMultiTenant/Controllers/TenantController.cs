using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using B2CMultiTenant.Extensions;
using B2CMultiTenant.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace B2CMultiTenant.Controllers
{
    [Authorize(Roles = "admin")]
    public class TenantController : Controller
    {
        public TenantController(RESTService rest)
        {
            _rest = rest;
        }
        RESTService _rest;
        public async Task<ActionResult> Edit()
        {
            var tenantIdClaim = User.FindFirst("appTenantId"); 
            if (tenantIdClaim != null)
            {
                var http = await _rest.GetClientAsync();
                var tenantId = tenantIdClaim.Value;
                var json = await http.GetStringAsync($"{RESTService.Url}/tenant/oauth2/{tenantId}");
                var group = JObject.Parse(json);
                return View(new TenantDetails
                {
                    Name = group["name"].Value<string>(),
                    LongName = group["description"].Value<string>()
                });
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind("Name,LongName, IsAADTenant, IdPDomainName")] TenantDetails tenant)
        {
            var tenantIdClaim = User.FindFirst("appTenantId");
            if (tenantIdClaim != null)
            {
                try
                {
                    var http = await _rest.GetClientAsync();
                    var tenantId = tenantIdClaim.Value;
                    var json = await http.PutAsync(
                        $"{RESTService.Url}/tenant/oauth2/{tenantId}",
                        new StringContent(JObject.FromObject(tenant).ToString(), Encoding.UTF8, "application/json"));
                    return RedirectToAction(nameof(Edit));
                }
                catch(Exception ex)
                {
                    throw;
                }
            }
            return View();
        }
    }
}