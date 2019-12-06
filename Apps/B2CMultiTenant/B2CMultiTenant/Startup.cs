using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using B2CMultiTenant.Models;
using B2CMultiTenant.Extensions;
using Microsoft.Identity.Client;
using System.Security.Claims;

namespace B2CMultiTenant
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddHttpContextAccessor();
            services.AddScoped<Extensions.TokenService>();
            services.AddTransient<RESTService>();
            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                    .AddCookie(options => {
                        options.LoginPath = "/Account/Unauthorized/";
                        options.AccessDeniedPath = "/Account/Forbidden/";
                    })
                    .AddOpenIdConnect("mtsusi", options => OptionsFor(options, "mtsusi"))
                    .AddOpenIdConnect("mtsusint", options => OptionsFor(options, "mtsusint"));
            services.Configure<ConfidentialClientApplicationOptions>(options => Configuration.Bind("AzureAD", options));

            services.AddSession(options => options.Cookie.IsEssential = true);
            services
                .AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            //.AddMvc(options =>
            //{
            //    var policy = new AuthorizationPolicyBuilder()
            //        .RequireAuthenticatedUser()
            //        .Build();
            //    options.Filters.Add(new AuthorizeFilter(policy));
            //})
            //services.AddAuthorization(options =>
            //{
            //    var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
            //        JwtBearerDefaults.AuthenticationScheme,
            //        "AzureAD");
            //    defaultAuthorizationPolicyBuilder =
            //        defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
            //    options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
            //});
        }

        private void OptionsFor(OpenIdConnectOptions options, string policy)
        {
            var aadOptions = new AzureADOptions();
            Configuration.Bind("AzureAD", aadOptions);
            options.ClientId = aadOptions.ClientId;
            var aadTenant = aadOptions.Domain.Split('.')[0];
            options.MetadataAddress = $"https://{aadTenant}.b2clogin.com/{aadOptions.TenantId}/b2c_1a_{policy}/v2.0/.well-known/openid-configuration";
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
            {
                NameClaimType = "name"
            };
            options.CallbackPath = new PathString($"/signin-{policy}"); // otherwise getting 'correlation error'
            options.SignedOutCallbackPath = new PathString($"/signout-{policy}");
            options.SignedOutRedirectUri = "/";
            options.ResponseType = OpenIdConnectResponseType.CodeIdToken;
            //TODO: Improve, Concat could not be used
            foreach(var s in RESTService.Scopes)
                options.Scope.Add(s);
            options.Events.OnAuthorizationCodeReceived = async (ctx) =>
            {
                ctx.HandleCodeRedemption();
                // The cache will need the claims from the ID token.
                // If they are not yet in the HttpContext.User's claims, so adding them here.
                if (!ctx.HttpContext.User.Claims.Any())
                {
                    (ctx.HttpContext.User.Identity as ClaimsIdentity).AddClaims(ctx.Principal.Claims);
                }
                var ts = ctx.HttpContext.RequestServices.GetRequiredService<Extensions.TokenService>();
                var auth = ts.AuthApp;
                var tokens = await auth.AcquireTokenByAuthorizationCode(
                    RESTService.Scopes,
                    ctx.ProtocolMessage.Code).ExecuteAsync().ConfigureAwait(false);
                ctx.HandleCodeRedemption(null, tokens.IdToken);
            };
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseSession();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
