using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.AspNetCore.Mvc;
using RESTFunctions.Services;

namespace RESTFunctions
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.Configure<ClientCertificateOptions>(Configuration.GetSection("AuthCert"));
            services.Configure<ConfidentialClientApplicationOptions>(Configuration.GetSection("ClientCreds"));
            services.AddSingleton<Services.Graph>();
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(jwtOptions =>
                {
                    jwtOptions.MetadataAddress = $"{Configuration["B2C:Instance"]}/{Configuration["B2C:TenantId"]}/{Configuration["B2C:Policy"]}/v2.0/.well-known/openid-configuration";
                    //jwtOptions.Authority = $"https://login.microsoftonline.com/tfp/{Configuration["B2C:TenantId"]}/{Configuration["B2C:Policy"]}/v2.0/";
                    jwtOptions.Audience = Configuration["B2C:ClientId"];
                    //jwtOptions.Events = new JwtBearerEvents
                    //{
                    //    OnAuthenticationFailed = AuthenticationFailed
                    //};
                });
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseCertificateValidator();
            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
