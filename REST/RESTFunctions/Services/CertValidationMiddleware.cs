using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
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
        public CertValidationMiddleware(RequestDelegate next, IOptionsMonitor<ClientCertificateOptions> options)
        {
            _next = next;
            _optionsMonitor = options;
        }
        private readonly RequestDelegate _next;
        private readonly IOptionsMonitor<ClientCertificateOptions> _optionsMonitor;
        public async Task InvokeAsync(HttpContext context)
        {
            Trace.WriteLine("Starting cert validation");
            bool isAuthorized = false;
            ClaimsIdentity identity  = null;
            var certHeader = context.Request.Headers["X-ARR-ClientCert"];
            if (!String.IsNullOrEmpty(certHeader))
            {
                Trace.WriteLine("Certificate present");
                try
                {
                    var options = _optionsMonitor.CurrentValue;
                    Trace.WriteLine($"Issuer: {options.issuer}; subject: {options.subject}");
                    if (options == null) throw new ApplicationException("Bad options");
                    byte[] clientCertBytes = Convert.FromBase64String(certHeader);
                    var certificate = new X509Certificate2(clientCertBytes);
                    //if (!certificate.Verify()) throw new ApplicationException("Verify failed");
                    if (DateTime.Compare(DateTime.Now, certificate.NotBefore) < 0 ||
                        DateTime.Compare(DateTime.Now, certificate.NotAfter) > 0)
                        throw new ApplicationException("Validity period");
                    Trace.WriteLine($"Date validated");
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
                    Trace.WriteLine($"System exception: {ex.Message}");
                    //context.Response.StatusCode = 500;
                    //await context.Response.WriteAsync("Failed");
                    //return;
                }
            }
            if (isAuthorized)
            {
                context.User = new System.Security.Claims.ClaimsPrincipal(identity);
            }
            //else
            //{
            //    context.Response.StatusCode = 403;
            //    await context.Response.WriteAsync("Unauthorized");
            //}
            // Call the next delegate/middleware in the pipeline
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
