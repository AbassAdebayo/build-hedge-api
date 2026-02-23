using Application.DTOs.HedgeContract;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories
{
    public interface IHedgeContractRepository : IBaseRepository
    {
        //Task<decimal> GetTotalPremiumRevenueAsync(bool global);
        Task<List<OrganizationRevenueDetails>> GetRevenuePerOrganization();

        Task<int> GetMonthlyHedgeCount(Guid organizationId);

        Task<IEnumerable<HedgeContract>> GetAllGlobalContractsAsync();

    }
}
