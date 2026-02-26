using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Messaging
{
    public interface IMailSender
    {
        Task<bool> Send(string from, string fromName, string to, string toName, string subject, string message, IDictionary<string, Stream> attachments = null!);
        public Task<bool> SendWithAttachment(string from, string fromName, string to, string toName, string subject, string message, byte[] attachmentContent, string fileName);
    }
}
