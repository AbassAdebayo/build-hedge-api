using Application.DTOs;
using Application.DTOs.HedgeContract;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Implementation
{
    public class PlatformRevenueService(IHedgeContractRepository hedgeContractRepository) : IPlatformRevenueService
    {
        private readonly IHedgeContractRepository _hedgeContractRepository = hedgeContractRepository ?? throw new ArgumentNullException(nameof(hedgeContractRepository));
        public async Task<BaseResponse<PlatformSummaryResponse>> GetOwnerDashboardAsync()
        {
            var totalRevenue = await _hedgeContractRepository.GetTotalPremiumRevenueAsync(global: true);
            var OrganizationBreakdown = await _hedgeContractRepository.GetRevenuePerOrganization();

            var result = new PlatformSummaryResponse(
            
                TotalPlatformRevenue: totalRevenue,
                TotalExposure: OrganizationBreakdown.Sum(x => x.TotalFeesPaid),
                TotalActiveContracts: OrganizationBreakdown.Sum(x => x.ContractCount),
                OrganizationBreakdown: OrganizationBreakdown
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
