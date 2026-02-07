using Domain.Contracts.Entities;
using Domain.Contracts.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class HedgeContract : BaseEntity
    {
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public Guid MaterialId { get; set; }
        public Material Material { get; set; } = null!;

        public decimal LockedPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal PremiumFee { get; set; }
        public DateTime ExpiryDate { get; set; }

        public ContractStatus Status { get; set; }
    }
}
