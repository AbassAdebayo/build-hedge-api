using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Messaging.Models
{
    public class SendVerification : Base
    {
        public string Token { get; set; }
    }
}
