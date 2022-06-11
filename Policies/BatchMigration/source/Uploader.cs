using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace UserMigration
{
    internal static class Uploader
    {
        private static IConfigurationRoot? Configuration;

        internal static async Task<int> RunAsync()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json")
                .AddJsonFile(@"C:\Users\mrochon\AppData\Roaming\Microsoft\UserSecrets\279dd2dc-f2d3-4864-8d78-f52f11ff9850\secrets.json");
                //.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                //.AddJsonFile("secrets.json");

            Configuration = builder.Build();
            var options = new ConfidentialClientApplicationOptions();
            Configuration.Bind("AzureB2C", options);

            var app = ConfidentialClientApplicationBuilder.CreateWithApplicationOptions(options)
                .Build();
            var tokens = app.AcquireTokenForClient(new string[] { ".default" }).ExecuteAsync().Result;

            var mailOptions = new MailOptions();
            Configuration.Bind("MailOptions", mailOptions);
            var mail = new SendGridClient(new SendGridClientOptions() { ApiKey = mailOptions.ApiKey });

            var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);

            var recordsLoaded = 0;
            var reader = new StreamReader("users.csv");
            if (reader == null)
            {
                Console.WriteLine("Data file not found");
                return 0;
            }
            var b2cName = Configuration.GetValue<string>("B2CName");
            var itemNames = reader.ReadLine().Split(',');
            while (!reader.EndOfStream)
            {
                var items = reader.ReadLine().Split(',');
                var user = new JsonObject();
                for(var i = 1; i < items.Length; i++)
                {
                    user.Add(itemNames[i], items[i]);
                }
                var identities = new object[] {
                    new {
                        signInType = "emailAddress",
                        issuer = $"{b2cName}.onmicrosoft.com",
                        issuerAssignedId = items[0]
                    }
                };
                user.Add("identities", JsonValue.Create(identities));
                var passwordProfile = new
                {
                    password = Uploader.Password(30),
                    forceChangePasswordNextSignIn = false
                };
                user.Add("passwordProfile", JsonValue.Create(passwordProfile));
                user.Add("passwordPolicies", "DisablePasswordExpiration");
                var json = user.ToString();
                var req = new HttpRequestMessage(HttpMethod.Post, "https://graph.microsoft.com/v1.0/users")
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
                var resp = await http.SendAsync(req);
                if (!resp.IsSuccessStatusCode)
                {
                    throw new Exception($"User: ${items[1]} - http error: ${await resp.Content.ReadAsStringAsync()} after {recordsLoaded} records loaded.");
                }
                json = await resp.Content.ReadAsStringAsync();
                var newUser = JsonSerializer.Deserialize<User>(json);
                Console.WriteLine($"Created {newUser?.id} - {items[0]}");
                var msg = new SendGridMessage()
                {
                    From = new EmailAddress(mailOptions.SenderEmail, mailOptions.SenderName),
                    Subject = "B2C signin"
                };
                var url = $"https://mrochonb2cprod.b2clogin.com/mrochonb2cprod.onmicrosoft.com/oauth2/v2.0/authorize?p=B2C_1A_CTP_PASSWORDRESETWITHHINT&client_id={mailOptions.B2CAppId}&nonce=defaultNonce&redirect_uri=https%3A%2F%2Foidcdebugger.com%2Fdebug&scope=openid&response_type=id_token&login_hint={items[0]}";
                msg.AddContent(MimeType.Html, $"<p>Please use this&sp;<a href='{url}'>link</a> to complete signin.</p>");
                msg.AddTo(new EmailAddress(items[0], ""));
                var response = await mail.SendEmailAsync(msg).ConfigureAwait(false);
                ++recordsLoaded;
            }
            return recordsLoaded;
        }

        static Random res = new Random();
        internal static string Password(int length)
        {
            var source = "abcdefghijklmnopqrstuvwxyz1234567890-=!@#$%^&*()_+<>?|}{";
            var ran = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                int n = res.Next(source.Length);
                ran = ran.Append(source[n]);
            }
            return ran.ToString();
        }
    }
    internal class User
    {
        public string id { get; set; }
    }
}

