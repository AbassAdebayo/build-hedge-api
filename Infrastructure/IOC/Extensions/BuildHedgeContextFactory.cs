using Infrastructure.Context;
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
            return new BuildHedgeContext(optionsBuilder.Options);
        }
    }
}
