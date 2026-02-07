using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Messaging.Models
{
    public class SendInvitation : Base
    {
        public string Role { get; set; }
        public string Token { get; set; }
    }
}
