using Domain.Contracts.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Organization : BaseEntity
    {
        public required string BusinessName { get; set; }
        public string? TaxId { get; set; }
        public string SubscriptionPlan { get; set; } = "Enterprise";

        public bool IsActive { get; set; }
        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<UserOrganizationMembership> Memberships { get; set; } = new List<UserOrganizationMembership>();
    }
}
