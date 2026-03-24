using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Messaging.Models
{
    public class SendInvoiceNotification : Base
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal AmountDue { get; set; }
        public DateTime DueDate { get; set; }
        public string BaseCurrency { get; set; }
        public string PortalLink { get; set; } = string.Empty;

        public bool IsReminder { get; set; }
        public bool IsPaid { get; set; }
    }
}
