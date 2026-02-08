using brevo_csharp.Api;
using brevo_csharp.Model;
using Domain.Messaging;
using Infrastructure.Exceptions.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Messaging
{
    public class MailSender(IConfiguration configuration, ILogger<MailSender> logger) : IMailSender
    {
        private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        private readonly ILogger<IMailSender> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public async Task<bool> Send(string from, string fromName, string to, string toName, string subject, string message, IDictionary<string, Stream> attachments = null)
        {
            var smtpApiKey = _configuration["BuildHedgeAPIs:SmtpApiKey"];

            var apiInstance = new TransactionalEmailsApi();
            var sendSmtpEmail = new SendSmtpEmail
            {
                HtmlContent = message,
                Subject = subject,
                Sender = new SendSmtpEmailSender(fromName, from),
                To = new List<SendSmtpEmailTo>() { new SendSmtpEmailTo(to, toName) }
            };

            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    sendSmtpEmail.Attachment.Add(new SendSmtpEmailAttachment(content: ReadFully(attachment.Value), name: attachment.Key));
                }
            }

            if (!string.IsNullOrEmpty(smtpApiKey))
            {
                brevo_csharp.Client.Configuration.Default.AddApiKey("api-key", smtpApiKey);
                try
                {
                    await apiInstance.SendTransacEmailAsync(sendSmtpEmail);
                    return true;
                }
                catch (Exception e)
                {
                    _logger.LogError("Exception when calling TransactionalEmailsApi.SendTransacEmail: " + e.Message);
                    throw new MailSenderException(e.Message, e);
                }
            }

            _logger.LogError("SMTP API Key is not configured.");
            throw new MailSenderException("SMTP API Key is not configured.");


        }

        private static byte[] ReadFully(Stream input)
        {
            using MemoryStream ms = new();
            input.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
