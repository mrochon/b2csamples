using B2CRestApis.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
