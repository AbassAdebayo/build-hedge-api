using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Messaging.Models
{
    public class SendNotification : Base
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string ActionText { get; set; } = "View My Hedges";
    }
}
