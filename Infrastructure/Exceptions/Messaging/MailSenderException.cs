using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Exceptions.Messaging
{
    public class MailSenderException : Exception
    {
        public string Message { get; set; }

        public MailSenderException(string message)
            : base(message) { }

        public MailSenderException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
