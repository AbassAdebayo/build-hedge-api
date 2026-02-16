using Application.Tenant;
using Infrastructure.Context;
using Infrastructure.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.IOC.Extensions
{
    internal class BuildHedgeContextFactory : IDesignTimeDbContextFactory<BuildHedgeContext>
    {
        public BuildHedgeContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BuildHedgeContext>();

            optionsBuilder.UseSqlServer("data source=(localdb)\\mssqllocaldb;initial catalog=BuildHedge_Db;integrated security=true;encrypt=false;trustservercertificate=true");

            // Provide a dummy ITenantProvider implementation for design-time context creation
            ITenantProvider tenantProvider = new DummyTenantProvider();

            return new BuildHedgeContext(optionsBuilder.Options, tenantProvider);
        }
    }

    // Dummy ITenantProvider implementation for design-time usage
    internal class DummyTenantProvider : ITenantProvider
    {
        public string GetTenantAdminName()
        {
            return string.Empty;
        }

        public Guid GetTenantId()
        {
            // Return a default or fixed tenant id for design-time
            return Guid.Empty;
        }
    }
}
