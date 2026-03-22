using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class BillingStatementRepository(BuildHedgeContext context) : BaseRepository(context), IBillingStatementRepository
    {
        private readonly BuildHedgeContext _context = context ?? throw new ArgumentNullException(nameof(context));
        public async Task<BillingStatement> GetBillingStatementWithOrganization(Guid id)
        {
            var invoice = await _context.Set<BillingStatement>()
                .IgnoreQueryFilters()
                .Include(b => b.Organization)
                .FirstOrDefaultAsync(b => b.Id == id);

            return invoice;
        }
    }
}
