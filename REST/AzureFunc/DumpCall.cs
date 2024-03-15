using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MR.AzureFuncs
{
    public class DumpCall
    {
        private readonly ILogger<DumpCall> _logger;

        public DumpCall(ILogger<DumpCall> logger)
        {
            _logger = logger;
        }

        [Function("DumpCall")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation(">>>>>>>DumpCall starting.<<<<<<<<");
            _logger.LogInformation($"Request method: {req.Method}");
            _logger.LogInformation("Dumping headers");
            foreach(var h in req.Headers)
            {
                _logger.LogInformation($"Header: {h.Key} = {h.Value}");
            }
            _logger.LogInformation("Dumping query parameters:");
            foreach(var p in req.Query)
            {
                _logger.LogInformation($"Query parameter: {p.Key} = {p.Value}");
            }
            _logger.LogInformation("Dumping body:");
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            _logger.LogInformation(body);
            return new OkResult();
        }
    }
}
