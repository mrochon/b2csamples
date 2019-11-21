using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2CMultiTenant.Controllers
{
    [Authorize(Roles="admin")]
    public class TenantAdminController : Controller
    {
        public IActionResult Index()
        {
            var u = User;
            return View();
        }
    }
}