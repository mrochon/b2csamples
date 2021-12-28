using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace RESTFunctions.Services
{
    public class CertValidationMiddleware
    {
        public CertValidationMiddleware(RequestDelegate next, IOptionsMonitor<ClientCertificateOptions> options, ILogger<CertValidationMiddleware> logger)
        {
            _next = next;
            _optionsMonitor = options;
            _logger = logger;
        }
        private readonly RequestDelegate _next;
        private readonly IOptionsMonitor<ClientCertificateOptions> _optionsMonitor;
        ILogger<CertValidationMiddleware> _logger;
        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("Starting cert validation");
            bool isAuthorized = false;
            ClaimsIdentity identity  = null;
            var certHeader = context.Request.Headers["X-ARR-ClientCert"];
            if (!String.IsNullOrEmpty(certHeader))
            {
                _logger.LogInformation("Certificate present");
                try
                {
                    var options = _optionsMonitor.CurrentValue;
                    if (options == null) throw new ApplicationException("Bad options");
                    _logger.LogInformation("Options:");
                    _logger.LogInformation($"Thumbprint: {options.thumbprint}");
                    _logger.LogInformation($"Subject: {options.subject}");
                    _logger.LogInformation($"Issuer: {options.issuer}");

                    byte[] clientCertBytes = Convert.FromBase64String(certHeader);
                    var certificate = new X509Certificate2(clientCertBytes);
                    //if (!certificate.Verify()) throw new ApplicationException("Verify failed");
                    if (DateTime.Compare(DateTime.Now, certificate.NotBefore) < 0 ||
                        DateTime.Compare(DateTime.Now, certificate.NotAfter) > 0)
                        throw new ApplicationException("Validity period");
                    _logger.LogInformation($"Date validated");
                    _logger.LogInformation($"Thumbprint: {certificate.Thumbprint}");
                    _logger.LogInformation($"Subject: {certificate.Subject.Trim()}");
                    _logger.LogInformation($"Issuer: {certificate.Issuer.Trim()}");
                    isAuthorized =
                        (String.Compare(certificate.Thumbprint, options.thumbprint, true, CultureInfo.InvariantCulture) == 0)
                        && (String.Compare(certificate.Subject.Trim(), options.subject, true, CultureInfo.InvariantCulture) == 0)
                        && (String.Compare(certificate.Issuer.Trim(), options.issuer, true, CultureInfo.InvariantCulture) == 0);
                    if (isAuthorized)
                    {
                        identity = new ClaimsIdentity(
                            new Claim[] { new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "ief") }) ;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"System exception: {ex.Message}");
                }
            }
            _logger.LogInformation($"Is authorized? {isAuthorized}");
            if (isAuthorized)
            {
                context.User = new System.Security.Claims.ClaimsPrincipal(identity);
            }
            await _next(context);
        }
    }

    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseCertificateValidator(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CertValidationMiddleware>();
        }
    }

    public class ClientCertificateOptions
    {
        public string thumbprint { get; set; }
        public string issuer { get; set; }
        public string subject { get; set; }
    }
}
