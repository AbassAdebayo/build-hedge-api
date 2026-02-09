using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.IOC.Extensions
{
    public static class DynamicCollection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddCustomIdentity(config)
                .AddRepositories()
            .AddServices();
            return services;
        }
    }
}
