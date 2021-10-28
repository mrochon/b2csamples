using B2CRestApis.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace B2CRestApis.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class Email : ControllerBase
    {
        private readonly ILogger<Email> _logger;
        private readonly IHttpClientFactory _clientFactory;
        public Email(ILogger<Email> logger, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _clientFactory = clientFactory;
        }
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Send([FromBody] OTPEmail otp)
        {
            using (_logger.BeginScope("Send email", otp))
            {
                using (var http = _clientFactory.CreateClient("O365"))
                {
                    var msg = new
                    {
                        message = new
                        {
                            subject = "OTP Code",
                            body = new
                            {
                                contentType = "Text",
                                content = $"Please use {otp.otp} to confirm your email address"
                            },
                            toRecipients = new[] { new { emailAddress = new { address = otp.email } } }
                        },
                        saveToSentItems = false
                    };
                    var resp = await http.SendAsync(
                        new HttpRequestMessage(HttpMethod.Post, http.BaseAddress + $"users/mrochon@meraridom.com/sendMail")
                        {
                            Content = new StringContent(JsonConvert.SerializeObject(msg), Encoding.UTF8, "application/json")
                        });
                    if (resp.IsSuccessStatusCode)
                        return Ok();
                    else
                        new Exception(resp.ReasonPhrase);
                }
            }
            return Ok();
        }
        [HttpGet("VerifySource")]
        [AllowAnonymous]
        public IActionResult Verify([FromQuery] string email, string ipAddress)
        {
            var transId = Guid.NewGuid().ToString();
            using (_logger.BeginScope("Validate"))
            {
                if (String.IsNullOrEmpty(email))
                    return BadRequest(new ErrorMsg { userMessage = "Invalid arguments" });
                _logger.LogDebug("{0}. Verify{1}-{2}", transId, email, ipAddress);
                var emailParts = email.Split('@');
                if (emailParts.Length != 2)
                {
                    _logger.LogError("{0}. Bad syntax", transId);
                    return BadRequest(new ErrorMsg { userMessage = "Invalid argument(s)" });
                }
                var domain = emailParts[1].ToLower();
                if (!Constants.Domains.Keys.Contains(domain))
                {
                    _logger.LogInformation("{0}. Invalid domain", transId);
                    return BadRequest(new ErrorMsg { userMessage = "Verification failed" });
                }
                if (null != Constants.Domains[domain])
                {
                    foreach (var CIDR in Constants.Domains[domain])
                    {
                        if (!IsInRange(ipAddress, CIDR))
                        {
                            _logger.LogInformation("{0}. Invalid or unexpected IP address", transId);
                            return BadRequest(new ErrorMsg { userMessage = "Verification failed" });
                        }
                    }
                }
            }
            return new OkResult();
        }

        // https://stackoverflow.com/questions/9622967/how-to-see-if-an-ip-address-belongs-inside-of-a-range-of-ips-using-cidr-notation
        // true if ipAddress falls inside the CIDR range, example
        // bool result = IsInRange("10.50.30.7", "10.0.0.0/8");
        private bool IsInRange(string ipAddress, string CIDRmask)
        {
            if (CIDRmask == "0.0.0.0/0") // all
                return true;
            if (String.IsNullOrEmpty(ipAddress))
                return false;
            IPAddress ip;
            if (!IPAddress.TryParse(ipAddress, out ip))
                return false;

            string[] parts = CIDRmask.Split('/');

            int IP_addr = BitConverter.ToInt32(IPAddress.Parse(ipAddress).GetAddressBytes(), 0);
            int CIDR_addr = BitConverter.ToInt32(IPAddress.Parse(parts[0]).GetAddressBytes(), 0);
            int CIDR_mask = IPAddress.HostToNetworkOrder(-1 << (32 - int.Parse(parts[1])));

            return ((IP_addr & CIDR_mask) == (CIDR_addr & CIDR_mask));
        }
    }
}
