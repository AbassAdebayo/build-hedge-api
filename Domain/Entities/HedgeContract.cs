using Domain.Contracts.Entities;
using Domain.Contracts.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class HedgeContract : BaseEntity
    {
        public Guid? CreatedByUserId { get; set; }
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;

        public Guid MaterialId { get; set; }
        public Material Material { get; set; } = null!;
        public Currency Currency { get; set; }
        public Guid CurrencyId { get; set; }
        public decimal ExchangeRateAtLock { get; set; }
        public decimal LockedPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal PremiumFee { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime? NoticeSentAt { get; set; }
        public DateTime? IsSevenDayNoticeSent { get; set; }
        public DateTime? IsFinalNoticeSent { get; set; }
        public ContractStatus Status { get; set; }
        public decimal TotalValueBaseCurrency { get; set; }
    }
}
