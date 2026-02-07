using Application.Interfaces.Identity;
using Application.Interfaces.Repositories;
using Application.Tenant;
using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.Identity;
using Infrastructure.Repositories;
using Infrastructure.Tenant;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.IOC.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<BuildHedgeContext>(options =>
                options.UseSqlServer(connectionString));
            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services.AddScoped<IUnitOfWork, UnitOfWork>()

                .AddScoped<IUserRepository, UserRepository>()
                .AddScoped<IBaseRepository, BaseRepository>();
                

        }

        public static IServiceCollection AddCustomIdentity(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IUserStore<User>, UserStore>();
            services.AddScoped<IRoleStore<Role>, RoleStore>();
            services.AddScoped<ITenantProvider, TenantProvider>();
            services.AddIdentity<User, Role>()
                .AddDefaultTokenProviders();
            return services;
        }
    }
}
