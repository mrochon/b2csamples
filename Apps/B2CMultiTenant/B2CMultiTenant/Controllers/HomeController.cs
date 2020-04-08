using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using B2CMultiTenant.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using B2CMultiTenant.Extensions;
using System.Security.Claims;
using System.Web;

namespace B2CMultiTenant.Controllers
{
    //[Authorize]
    public class HomeController : Controller
    {
        public HomeController(TokenService tokenService)
        {
            _tokenService = tokenService;
        }
        TokenService _tokenService;
        public IActionResult Index()
        {
            //if (User.FindFirst(c => c.Type == "isNewMember")?.Value == "true")
            if (Request.Query.ContainsKey("tenant"))
            {
                var authParms = new AuthenticationProperties() { RedirectUri = "/Home/Index" };
                authParms.Parameters.Add("tenant", (string)Request.Query["tenant"]);
                if (Request.Query.ContainsKey("domain"))
                    authParms.Parameters.Add("domain", (string)Request.Query["domain"]);
                return Challenge(authParms,
                    new string[] { "mtsusi2" });
            } else if (User.Identity.IsAuthenticated)
            {
                var tenantName = User.FindFirst(c => c.Type == "appTenantName")?.Value;
                ViewBag.ReturnUrl = $"{Request.Scheme}://{Request.Host}?tenant={tenantName}";
                if (User.HasClaim(c => c.Type == "http://schemas.microsoft.com/identity/claims/identityprovider"))
                    ViewBag.ReturnUrl += $"&domain={User.FindFirstValue("http://schemas.microsoft.com/identity/claims/identityprovider")}";
            }
            return View(User.Claims);
        }
        public IActionResult MemberSignIn()
        {
            var authParms = new AuthenticationProperties() { RedirectUri = "/Home/Index" };
            if (Request.Query.ContainsKey("domain"))
                authParms.Parameters.Add("domain", (string)Request.Query["domain"]);
            return Challenge(authParms,
                new string[] { "mtsusi-firsttenant" });
            /*if (Request.Query.ContainsKey("tenant"))
            {
                var authParms = new AuthenticationProperties() { RedirectUri = "/Home/Index" };
                authParms.Parameters.Add("tenant", Request.Query["tenant"][0]);
                return Challenge(authParms,
                    new string[] { "mtsusi2" });
            };
            return View();*/
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MemberSignIn(string tenantName)
        {
            return RedirectToAction("MemberSignIn", "Home", new { tenant = tenantName });
        }
        public IActionResult NewTenant()
        {
            return Challenge(
                new AuthenticationProperties() { RedirectUri = "/Home/Index" },
                new string[] { "mtsusint" });
        }
        [Authorize]
        public IActionResult SignOut()
        {
            var loginPath = User.Claims.First(c => c.Type == "http://schemas.microsoft.com/claims/authnclassreference").Value;
            var policy = loginPath.Split('_')[2];
            return SignOut(
                new AuthenticationProperties() { RedirectUri = "/Home/Index" },
                new string[] { CookieAuthenticationDefaults.AuthenticationScheme, policy });
        }
        public IActionResult PwdReset()
        {
            return Challenge(
                new AuthenticationProperties() { RedirectUri = "/Home/Index" },
                new string[] { "mtpasswordreset" });
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            if (Request.Query.ContainsKey("message"))
            {
                var message = Request.Query["message"][0];
                ViewBag.Message = HttpUtility.UrlDecode(message);
                return View();
            } else
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
