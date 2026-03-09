#nullable enable
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class GlobalConfigurationRepository(BuildHedgeContext context) : BaseRepository(context), IGlobalConfigurationRepository
    {
        private readonly BuildHedgeContext _context = context ?? throw new ArgumentNullException(nameof(context));
        public async Task<string?> GetValueByKeyAsync(string key)
        {
            var configEntry = await _context.Set<GlobalSettings>()
                .FirstOrDefaultAsync(gc => gc.Key == key);

            return configEntry?.Value;
        }
    }
}
