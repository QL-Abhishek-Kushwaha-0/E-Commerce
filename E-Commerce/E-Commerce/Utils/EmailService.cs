using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Blog_Application.Utils
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendMail(string userMail, string userName, string emailSubject, string emailData)
        {
            var smtpSettings = _config.GetSection("SmtpSettings");      // Importing the Configuration Details for Smtp Settings

            // Creating the structue of Email Message to Send

            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("Faison Hub", smtpSettings["Email"]));
            message.To.Add(new MailboxAddress(userName, userMail));

            message.Subject = $"{emailSubject}";

            message.Body = new TextPart("html")
            {
                Text = $@"
                <h2>Welcome, {userName}!</h2>
                {emailData}"
            };

            // Generating Access Token
            string accessToken = await GetAccessTokenAsync(smtpSettings["ClientId"], smtpSettings["ClientSecret"]);

            using var client = new SmtpClient();

            await client.ConnectAsync(smtpSettings["Host"], int.Parse(smtpSettings["Port"]), MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(new SaslMechanismOAuth2(smtpSettings["Email"], accessToken));
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }


        private async Task<string> GetAccessTokenAsync(string clientId, string clientSecret)
        {
            string[] scopes = { "https://mail.google.com/" };
            UserCredential credential;

            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore("token.json", true),
                    new FixedPortCodeReceiver() // Uses fixed port 5000
                );
            }

            // Refreshing the access_token in case it is expired
            if (credential.Token.IsStale)
            {
                if (string.IsNullOrEmpty(credential.Token.RefreshToken))
                {
                    throw new Exception("No refresh token available. Please re-authenticate.");
                }

                bool refreshed = await credential.RefreshTokenAsync(CancellationToken.None);
                if (!refreshed)
                {
                    throw new Exception("Failed to refresh token. Please re-authenticate.");
                }
            }

            return credential.Token.AccessToken;
        }
    }
}
