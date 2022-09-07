using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using static System.Formats.Asn1.AsnWriter;
using System.Net.Http.Headers;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAdB2C"));
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IConfidentialClientApplication>(svc =>
{
    var graphClientOptions = new ConfidentialClientApplicationOptions();
    builder.Configuration.Bind("AzureAdB2C", graphClientOptions);
    graphClientOptions.Instance = "https://login.microsoftonline.com";
    return ConfidentialClientApplicationBuilder.CreateWithApplicationOptions(graphClientOptions).Build();
});
builder.Services.AddScoped<GraphServiceClient>(svc =>
{
    return new GraphServiceClient("https://graph.microsoft.com/V1.0/", new DelegateAuthenticationProvider(async (requestMessage) =>
    {
        var msal = svc.GetService<IConfidentialClientApplication>();
        AuthenticationResult result = await msal.AcquireTokenForClient(
            new string[]
            {
                "https://graph.microsoft.com/.default"
            })
            .ExecuteAsync();
        requestMessage.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", result.AccessToken);
    }));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
