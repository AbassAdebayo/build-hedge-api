using Application.DTOs;
using Application.DTOs.HedgeContract;
using Application.ExchangeRate;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Implementation
{
    public class PlatformRevenueService(IHedgeContractRepository hedgeContractRepository,
        IGlobalConfigurationService globalConfiguration, ICurrencyExchangeService currencyExchange) : IPlatformRevenueService
    {
        private readonly IHedgeContractRepository _hedgeContractRepository = hedgeContractRepository ?? throw new ArgumentNullException(nameof(hedgeContractRepository));
        private readonly IGlobalConfigurationService _globalConfiguration = globalConfiguration ?? throw new ArgumentNullException(nameof(globalConfiguration));
        private readonly ICurrencyExchangeService _exchangeService = currencyExchange ?? throw new ArgumentNullException(nameof(currencyExchange));
        public async Task<BaseResponse<PlatformSummaryResponse>> GetOwnerDashboardAsync()
        {
            // Get owner's currency
            var systemBaseCurrency = await _globalConfiguration.GetSystemBaseCurrency();

            var contracts = await _hedgeContractRepository.GetAllGlobalContractsAsync();

            decimal totalPlatformRevenue = 0.0m;

            var orgDetails = new List<OrganizationRevenueDetails>();

            var groupedByOrg = contracts.GroupBy(c => c.Organization);

            foreach(var group in groupedByOrg)
            {
                var org = group.Key;
                decimal orgTotalInOwnerCurrency = 0;

                foreach(var contract in group)
                {
                    var rateResponse = await _exchangeService.GetExchangeRateAsync(org.BaseCurrencyCode, systemBaseCurrency);

                    decimal rate = rateResponse.IsSuccess ? rateResponse.Data : 1.0m;
                    orgTotalInOwnerCurrency += (contract.PremiumFee * rate);
                }

                totalPlatformRevenue += orgTotalInOwnerCurrency;

                orgDetails.Add( new OrganizationRevenueDetails(
                    org.BusinessName,
                    orgTotalInOwnerCurrency,
                    group.Count()
                 ));


            }

            //var totalRevenue = await _hedgeContractRepository.GetTotalPremiumRevenueAsync(global: true);
            //var OrganizationBreakdown = await _hedgeContractRepository.GetRevenuePerOrganization();

            var result = new PlatformSummaryResponse(
            
                TotalPlatformRevenue: totalPlatformRevenue,
                TotalExposure: orgDetails.Sum(x => x.TotalFeesPaid),
                TotalActiveContracts: contracts.Count(),
                OrganizationBreakdown: orgDetails.OrderByDescending(o => o.TotalFeesPaid).ToList(),
                CurrencyCode: systemBaseCurrency

            );

            return new BaseResponse<PlatformSummaryResponse>
            (
                Message: "Platform revenue summary retrieved successfully.",
                Status: true,
                Data: result
            );
        }
    }
}
