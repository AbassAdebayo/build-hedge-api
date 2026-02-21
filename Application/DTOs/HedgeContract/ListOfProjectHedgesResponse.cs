using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.HedgeContract
{
    public record ListOfProjectHedgesResponse(
         Guid Id,
         decimal Quantity,
         decimal LockedPrice,
         decimal PremiumFee,
         DateTime ExpiryDate,
         decimal TotalValueBaseCurrency
      );

    public record PlatformSummaryResponse(
        decimal TotalPlatformRevenue,
        decimal TotalExposure,
        int TotalActiveContracts,
        List<OrganizationRevenueDetails> OrganizationBreakdown
    );
     public record OrganizationRevenueDetails(
        string OrganizationName,
        decimal TotalFeesPaid,
        int ContractCount
     );



}
