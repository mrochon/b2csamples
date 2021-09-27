using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CreateInvitation
{
    class Program
    {

        static Task Main(string[] args)
        {
            using IHost host = Host.CreateDefaultBuilder(args).UseEnvironment("Development")
                .ConfigureAppConfiguration((hostContext, configuration) =>
                {
                    configuration.Sources.Clear();
                    if (hostContext.HostingEnvironment.IsDevelopment())
                    {
                        configuration.AddUserSecrets<Program>();
                    }
                    var env = hostContext.HostingEnvironment;
                    configuration
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    var configurationRoot = configuration.Build();
                    configurationRoot.GetSection("Invitation").Bind(options);
                })
                .ConfigureServices((ctx, services) =>
                {
                    services.AddTransient<InvitationService>(svc =>
                    {
                        InvitationTokenOptions options = new();
                        ctx.Configuration.Bind("Invitation", options);
                        return new InvitationService(options);
                    });
                })
                .Build();

            var svc = host.Services.GetService<InvitationService>();
            /*
            var app = PublicClientApplicationBuilder
            .Create(ClientId)
                .WithAuthority(Authority)
                .WithRedirectUri("http://localhost")
                .Build();
            var tokens = app.AcquireTokenInteractive(new string[] { "openid" }).WithUseEmbeddedWebView(true).ExecuteAsync().Result;
            */

            Console.WriteLine(svc.GetInvitationUrl(new InvitationDetails { inviteEmail = args[0] }));
            return host.RunAsync();
        }
        public static InvitationTokenOptions options = new();
    }
}
