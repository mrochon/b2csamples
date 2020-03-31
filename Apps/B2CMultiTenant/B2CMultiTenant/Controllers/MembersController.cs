using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using B2CMultiTenant.Models;
using Microsoft.AspNetCore.Authorization;
using B2CMultiTenant.Extensions;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;

namespace B2CMultiTenant.Controllers
{
    [Authorize]
    public class MembersController : Controller
    {
        public MembersController(RESTService rest, InvitationService invite)
        {
            _rest = rest;
            _invite = invite;
        }
        RESTService _rest;
        InvitationService _invite;
        // GET: Members
        public async Task<IActionResult> Index()
        {
            var tenantIdClaim = User.FindFirst("appTenantId"); // pwd reset does not return it;
            if (tenantIdClaim != null)
            {
                var http = await _rest.GetClientAsync();
                var tenantId = tenantIdClaim.Value;
                var json = await http.GetStringAsync($"{RESTService.Url}/tenant/oauth2/{tenantId}/members");
                var members = JArray.Parse(json).Select(m => new Member
                {
                    Id = m["userId"].Value<string>(),
                    Roles = (m["roles"].ToList().Select(t => t.Value<string>()).Aggregate((i, r) => $"{i}, {r}")),
                    DisplayName = m["name"].Value<string>()
                }).ToList();
                return View(members);
            }
            return View();
        }

        [Authorize(Roles ="admin")]
        public IActionResult Invite()
        {
            return View(new Invitee());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles ="admin")]
        public IActionResult Invite([Bind("Email,IsAdmin")] Invitee invitee)
        {
            if (ModelState.IsValid)
            {
                var role = invitee.IsAdmin ? "admin" : "member";
                invitee.InvitationUrl = _invite.GetInvitationUrl(invitee.Email, new Dictionary<string, string>()
                {
                    {"tenant", User.FindFirst("appTenantName").Value },
                    {"roles",  $"[\"{role}\"]" }
                });
                return View(invitee);
                //return RedirectToAction(nameof(Invite));
            }
            return View(new Invitee());
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Redeem(string id_token)
        {
            // will not validate the token since we will send the user back for signin anyway
            // Occassionally getting B2C error that user does not exists - presumably timing error between session state, which things user exists and b2C data where user not yet created.
            await Task.Delay(10000); // wait 10s
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(id_token);
            var tenant = token.Claims.FirstOrDefault(c => c.Type == "appTenantName");
            if (tenant != null)
            {
                var authParms = new AuthenticationProperties() { RedirectUri = "/Home/Index" };
                authParms.Parameters.Add("tenant", tenant.Value);
                return Challenge(authParms,
                    new string[] { "mtsusi2" });
            }
            return RedirectToAction("Index", "Home");
        }

        /*

        // GET: Members/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Member
                .FirstOrDefaultAsync(m => m.Id == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // GET: Members/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Members/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Role,DisplayName")] Member member)
        {
            if (ModelState.IsValid)
            {
                _context.Add(member);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(member);
        }

        // GET: Members/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Member.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            return View(member);
        }

        // POST: Members/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Role,DisplayName")] Member member)
        {
            if (id != member.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(member);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberExists(member.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(member);
        }

        // GET: Members/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Member
                .FirstOrDefaultAsync(m => m.Id == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // POST: Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var member = await _context.Member.FindAsync(id);
            _context.Member.Remove(member);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MemberExists(string id)
        {
            return _context.Member.Any(e => e.Id == id);
        }
        */

    }
}
