using Domain.Contracts.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Project : BaseEntity
    {
        public required string Name { get; set; }
        public decimal TotalBudget { get; set; }
        public DateTime EstimatedCompletion { get; set; }

        // Relationship to Organization
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;

        public ICollection<HedgeContract> HedgeContracts { get; set; } = new List<HedgeContract>();
    }
}
