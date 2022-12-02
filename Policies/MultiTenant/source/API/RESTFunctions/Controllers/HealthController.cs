using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RESTFunctions.Models;

namespace RESTFunctions.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HealthController> _logger;
        public HealthController(ILogger<HealthController> logger, IConfiguration conf)
        {
            _configuration= conf;
            _logger = logger;
        }
        // GET: api/Health
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet("props")]
        public string[] Props()
        {
            var inv = new InvitationTokenOptions();
            _configuration.GetSection("Invitation").Bind(inv);
            return new string[] { inv.Policy, inv.ValidityHours.ToString() };
        }

        // GET: api/Health/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Health
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Health/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
