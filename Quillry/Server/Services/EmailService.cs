using Mailjet.Client;
using Mailjet.Client.Resources;
using Newtonsoft.Json.Linq;

namespace Quillry.Server.Services
{
    public class EmailService : IEmailService
    {
        readonly ILogger<EmailService> _logger;
        readonly IConfiguration _config;
        readonly string _apiKey;
        readonly string _apiSecret;
        readonly string _sendFrom;

        public EmailService(ILogger<EmailService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            _apiKey = _config.GetSection("MailJetSettings").GetValue<string>("ApiKey");
            _apiSecret = _config.GetSection("MailJetSettings").GetValue<string>("ApiSecret");
            _sendFrom = _config.GetSection("MailJetSettings").GetValue<string>("SendFrom");
        }

        public async Task SendEmailAsync(string fromEmail, string toEmail, string subject, string message)
        {
            try
            {
                var response = await SendMailJetEmail(fromEmail, toEmail, subject, message);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(response.GetErrorMessage());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        private async Task<MailjetResponse> SendMailJetEmail(string fromEmail, string toEmail, string subject, string message)
        {
            MailjetClient client = new MailjetClient(_apiKey, _apiSecret);

            MailjetRequest request = new MailjetRequest
            {
                Resource = Send.Resource,
            }
            .Property(Send.FromEmail, fromEmail)
            .Property(Send.FromName, "Quillry")
            .Property(Send.Subject, subject)
            .Property(Send.HtmlPart, message)
            .Property(Send.Recipients, new JArray {
                new JObject {
                    {"Email", toEmail}
                }
             });

            return await client.PostAsync(request);
        }
    }
}
