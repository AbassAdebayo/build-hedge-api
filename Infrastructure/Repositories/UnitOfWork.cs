using Application.Interfaces.Repositories;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class UnitOfWork(BuildHedgeContext context) : IUnitOfWork
    {
        private readonly BuildHedgeContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public IExecutionStrategy CreateExecutionStrategy()
        {
            return _context.Database.CreateExecutionStrategy();
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
           return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
