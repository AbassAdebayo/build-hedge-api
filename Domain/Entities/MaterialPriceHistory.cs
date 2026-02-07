using Domain.Contracts.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class MaterialPriceHistory : BaseEntity
    {
        public Guid MaterialId { get; set; }
        public Material Material { get; set; } = null!;

        public decimal Price { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
