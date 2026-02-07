using Domain.Contracts.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Material : BaseEntity
    {
#pragma warning disable CS8618 
        public string Name { get; set; }
#pragma warning restore CS8618
        public required string TickerSymbol { get; set; }
        public string Unit { get; set; } = "Ton";

        // Metadata for AI (Sentiment, Tariff codes)
        public string? MetadataJson { get; set; }

        public ICollection<MaterialPriceHistory> PriceHistories { get; set; } = new List<MaterialPriceHistory>();
    }
}
