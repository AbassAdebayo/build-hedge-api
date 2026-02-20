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
    
}
