using Application.Implementation;
using Application.Interfaces.Identity;
using Application.Interfaces.IDS;
using Application.Interfaces.Payment;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Contracts.MailingServices;
using Domain.Contracts.PdfHandler;
using Domain.Contracts.Tenant;
using Domain.Entities;
using Domain.Messaging;
using Domain.TemplateEngine;
using Infrastructure.Context;
using Infrastructure.ExchangeRate;
using Infrastructure.Identity;
using Infrastructure.IDS;
using Infrastructure.Messaging;
using Infrastructure.PdfHandler;
using Infrastructure.Repositories;
using Infrastructure.Services.MailingService;
using Infrastructure.Services.Payment;
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
                .AddScoped<IMaterialService, MaterialService>()
                .AddScoped<IHedgeContractService, HedgeContractService>()
                .AddScoped<IPlatformRevenueService, PlatformRevenueService>()
                .AddScoped<IGlobalConfigurationService, GlobalConfigurationService>()
                .AddScoped<IIdsService, IdsService>()
                .AddScoped<IPaystackService, PaystackService>()
                .AddScoped<IBillingStatementService, BillingStatementService>()
                .AddScoped<IPdfService, PdfService>()
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
                .AddScoped<IMaterialRepository, MaterialRepository>()
                .AddScoped<IHedgeContractRepository, HedgeContractRepository>()
                .AddScoped<IGlobalConfigurationRepository, GlobalConfigurationRepository>()
                .AddScoped<IProcessPaymentRepository,  ProcessPaymentRepository>()
                .AddScoped<IBillingStatementRepository, BillingStatementRepository>()
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
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);         
                }));
            return services;
        }
    }
}
