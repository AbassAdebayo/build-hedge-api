using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.HedgeContract
{
    public record CreateHedgeContractRequestModel(
        Guid ProjectId,
        List<HedgeItemRequest> MaterialsToHedge
    );

    public record HedgeItemRequest(
        Guid MaterialId,
        Guid CurrencyId,
        decimal Quantity,
        decimal LockedPrice,
        DateTime ExpiryDate
    );

    public record BulkHedgeResponse(
        decimal GrandTotalMaterialCost,
        decimal GrandTotalPremiumFee,
        List<HedgeItemDetail> Items

        );

    public record HedgeItemDetail(
    Guid MaterialId,
    decimal CalculatedPremium,
    decimal TotalCostWithPremium,
    decimal ExchangeRate
);

}
