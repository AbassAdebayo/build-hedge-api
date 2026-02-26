using Domain.Contracts.Entities;
using Domain.Contracts.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Organization : BaseEntity
    {
        public required string BusinessName { get; set; }
        public string? TaxId { get; set; }
        public SubscriptionPlan SubscriptionPlan { get; set; }
        public string BaseCurrencyCode { get; set; } = null!;
        public DateTime? SubscriptionExpiryDate { get; set; }
        public bool IsInTrial { get; set; } 
        public DateTime? TrialExpiryDate { get; set; }
        public decimal AccruedFees { get; set; }
        public bool IsActive { get; set; }
        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<UserOrganizationMembership> Memberships { get; set; } = new List<UserOrganizationMembership>();
    }
}
