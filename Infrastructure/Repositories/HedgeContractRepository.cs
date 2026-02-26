using Application.DTOs.HedgeContract;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class HedgeContractRepository(BuildHedgeContext context) : BaseRepository(context), IHedgeContractRepository
    {
        private readonly BuildHedgeContext _context = context ?? throw new ArgumentNullException(nameof(context));
        //public async Task<decimal> GetTotalPremiumRevenueAsync(bool global)
        //{
          
        //    var query = _context.Set<HedgeContract>().AsQueryable();

        //    if(global) query = query.IgnoreQueryFilters();

        //    return query.Sum(hc => hc.PremiumFee);
        //}
        public async Task<List<OrganizationRevenueDetails>> GetRevenuePerOrganization()
        {
            
            return await _context.Set<Organization>()
                .IgnoreQueryFilters()
                .Select(org => new OrganizationRevenueDetails(
                
                    org.BusinessName,
                    _context.Set<HedgeContract>()
                        .IgnoreQueryFilters()
                        .Where(hc => hc.OrganizationId == org.Id)
                        .Sum(hc => hc.PremiumFee),
                    _context.Set<HedgeContract>()
                    .IgnoreQueryFilters()
                    .Where(h => h.OrganizationId == org.Id)
                    .Count()
                ))
                .ToListAsync();
        }

        public Task<int> GetMonthlyHedgeCount(Guid organizationId)
        {
            var firstDayOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            return _context.Set<HedgeContract>()
                .Where(hc => hc.OrganizationId == organizationId && hc.CreatedAtUtc >= firstDayOfMonth)
                .CountAsync();
        }

        public async Task<IEnumerable<HedgeContract>> GetAllGlobalContractsAsync()
        {
            return await _context.Set<HedgeContract>()
                .IgnoreQueryFilters()
                .Include(h => h.Organization)
                .ToListAsync();
        }

        public Task<List<HedgeContract>> GetHedgesForBilling(Guid organizationId, int month, int year)
        {
            return _context.Set<HedgeContract>()
                .Where(hc => hc.OrganizationId == organizationId 
                    && hc.CreatedAtUtc.Month == month 
                    && hc.CreatedAtUtc.Year == year)
                .OrderByDescending(hc => hc.CreatedAtUtc)
                .ToListAsync();
        }
    }
}
