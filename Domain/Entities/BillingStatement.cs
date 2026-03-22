using Domain.Contracts.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class BillingStatement : BaseEntity
    {
        public Guid OrganizationId { get; set; }
        public Organization? Organization { get; set; }
        public required string InvoiceNumber { get; set; }
        public decimal SubscriptionBaseFee { get; set; }
        public decimal TotalPremiumFees { get; set; } 
        public decimal TotalOverageFees { get; set; }


        public decimal TotalAmountDue => SubscriptionBaseFee + TotalPremiumFees + TotalOverageFees;

        public DateTime DueDate { get; set; }
        public bool IsPaid { get; set; } = false;
        public DateTime IsPaidAt { get; set; }

        public List<HedgeContract> HedgesIncluded { get; set; } = new();
    }
}
