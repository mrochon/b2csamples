using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace RESTFunctions.Services
{
    public class CertValidationMiddleware
    {
        public CertValidationMiddleware(RequestDelegate next, IConfiguration conf)
        {
            _next = next;
            _thumbprint = conf.GetValue<string>("AuthCert").ToLower();
        }
        private readonly RequestDelegate _next;
        private readonly string _thumbprint;
        public async Task InvokeAsync(HttpContext context)
        {
            bool isAuthorized = false;
            var certHeader = context.Request.Headers["X-ARR-ClientCert"];
            if (!String.IsNullOrEmpty(certHeader))
            {
                try
                {
                    byte[] clientCertBytes = Convert.FromBase64String(certHeader);
                    var certificate = new X509Certificate2(clientCertBytes);
                    isAuthorized = (String.Compare(certificate.Thumbprint, _thumbprint, true, System.Globalization.CultureInfo.InvariantCulture) == 0);
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("Failed");
                    return;
                }
            }
            if (isAuthorized)
            {
                // Call the next delegate/middleware in the pipeline
                await _next(context);
            }
            else
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Unauthorized");
            }
        }
    }

    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseCertificateValidator(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CertValidationMiddleware>();
        }
    }
}
