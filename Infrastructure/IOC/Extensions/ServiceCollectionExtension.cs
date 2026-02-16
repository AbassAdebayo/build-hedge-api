using Application.Implementation;
using Application.Interfaces.Identity;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Tenant;
using Domain.Contracts.MailingServices;
using Domain.Entities;
using Domain.Messaging;
using Domain.TemplateEngine;
using Infrastructure.Context;
using Infrastructure.Identity;
using Infrastructure.MailingService;
using Infrastructure.Messaging;
using Infrastructure.Repositories;
using Infrastructure.TemplateEngine;
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
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                
                .AddScoped<IOrganizationService, OrganizationService>()
                .AddScoped<IUserService, UserService>()
                .AddScoped<IUserOrganizationMembershipService, UserOrganizationMembershipService>()
                .AddScoped<IRoleService, RoleService>()
                .AddScoped<IProjectService, ProjectService>()
                .AddScoped<ICurrencyService, CurrencyService>()
                .AddScoped<IMailSender, MailSender>()
                .AddScoped<IMailService, MailService>()
                .AddScoped<IRazorEngine, RazorEngine>();
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services

                .AddScoped<IUnitOfWork, UnitOfWork>()
                .AddScoped<IUserRepository, UserRepository>()
                .AddScoped<IBaseRepository, BaseRepository>()
                .AddScoped<IOrganizationRepository, OrganizationRepository>()
                .AddScoped<IRoleRepository, RoleRepository>()
                .AddScoped<IProjectRepository, ProjectRepository>()
                .AddScoped<ICurrencyRepository, CurrencyRepository>()
                .AddScoped<IUserOrganizationMembershipRepository, UserOrganizationMembershipRepository>();



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

        public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<BuildHedgeContext>(options =>
                options.UseSqlServer(connectionString));
            return services;
        }
    }
}
