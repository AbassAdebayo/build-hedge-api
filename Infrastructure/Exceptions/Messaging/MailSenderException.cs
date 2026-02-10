using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Exceptions.Messaging
{
    public class MailSenderException : Exception
    {
#pragma warning disable CS0114
        public string Message { get; set; }
#pragma warning restore CS0114 

        public MailSenderException(string message)
            : base(message) { }

        public MailSenderException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
