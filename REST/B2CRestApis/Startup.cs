using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using B2CRestApis.Services;
using System.Net.Http;
using Microsoft.OpenApi.Models;
using B2CRestApis.Models;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace B2CRestApis
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
            services.Configure<BasicAuthOptions>(Configuration.GetSection("BasicAuth"));
            services.Configure<InvitationTokenOptions>(Configuration.GetSection("Invitation"));
            services.AddScoped<IBasicAuthenticationService, SymmetricKeyAuthentication>();
            services.AddTransient<Services.InvitationService>();
            services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme) // JwtBearerDefaults.AuthenticationScheme) //BasicAuthenticationDefaults.AuthenticationScheme)
                .AddBasicAuthentication()
                .AddJwtBearer(options =>
                {
                    options.MetadataAddress = "https://mrochonb2cprod.b2clogin.com/mrochonb2cprod.onmicrosoft.com/v2.0/.well-known/openid-configuration?p=B2C_1_BasicSUSI";
                    options.Audience = "5e976aba-65ee-4185-8fdc-d317f7c34959";
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                    {
                        NameClaimType = "name"
                    };
                });
            //    .AddMicrosoftIdentityWebApi(Configuration.GetSection("B2C"));
            //services.Configure<MicrosoftIdentityOptions>(options =>
            //{
            //    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
            //    {
            //        ValidateIssuer = false
            //    };
            //});

            services.AddHttpClient("B2C", (s,c) => SetupGraphClient("B2C", c));
            services.AddHttpClient("O365", (s, c) => SetupGraphClient("O365", c));

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "B2C REST APIs", Version = "v1" });
            });
        }

        private void SetupGraphClient(string sectionName, HttpClient c)
        {
            var options = new ConfidentialClientApplicationOptions();
            Configuration.Bind(sectionName, options);
            var msal = ConfidentialClientApplicationBuilder
                .CreateWithApplicationOptions(options)
                .Build();
            var tokens = msal.AcquireTokenForClient(new string[] { "https://graph.microsoft.com/.default" }).ExecuteAsync().Result;
            c.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);
            c.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
            c.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "REST v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
