using Domain.Contracts.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class ProcessedPayment: BaseEntity
    {
        public required string Reference { get; set; } 

        public Guid OrganizationId { get; set; }
        public Guid BillingStatementId { get; set; }

        public decimal AmountPaid { get; set; }
        public required string Currency { get; set; }

        public DateTime ProcessedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
