using Application.Interfaces.Identity;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Contracts.Tenant;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Infrastructure.Middleware
{
    public class SubscriptionMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));

        public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
        {
            // Skip check for non-API routes or specific endpoints (Auth, etc.)
            var path = context.Request.Path.Value?.ToLower();
            if (path != null && (path.Contains("/auth/") || path.Contains("/public/")))
            {
                await _next(context);
                return;
            }

            // 2. Create a scope to resolve Scoped Services (Repositories)
            using (var scope = serviceProvider.CreateScope())
            {
                var identityContext = scope.ServiceProvider.GetRequiredService<IIdentityService>();
                var hedgeContext = scope.ServiceProvider.GetRequiredService<IHedgeContractRepository>();
                var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantProvider>();
                var userMembershipContext = scope.ServiceProvider.GetRequiredService<IUserOrganizationMembershipRepository>();
                var organizationContext = scope.ServiceProvider.GetRequiredService<IOrganizationRepository>();
                var globalConfig = scope.ServiceProvider.GetRequiredService<IGlobalConfigurationService>();

                var currentTenantId = tenantContext.GetTenantId();
                var currentUserId = tenantContext.GetTenantUserId();

                var loggedInUser = identityContext.GetLoggedInUser();

                if (loggedInUser is not null)
                {
                    var currentUser = await userMembershipContext.Get<UserOrganizationMembership>(m => m.UserId == currentUserId);
                    var organization = await organizationContext.Get<Organization>(o => o.Id == currentTenantId);

                    // Check if the Subscription (Membership) is still valid
                    bool isSubscriptionExpired = DateTime.UtcNow > organization.SubscriptionExpiryDate;
                    bool isUnderTrial = DateTime.UtcNow <= organization.CreatedAtUtc.AddDays(14);

                    int currentMonthCount = await hedgeContext.GetMonthlyHedgeCount(organization.Id);
                    int maxAllowed = await globalConfig.GetHedgeQuotaAsync(organization.SubscriptionPlan);

                    // Fetch credit limit for the organization's subscription plan and check if they are over the limit
                    decimal creditLimit = await globalConfig.GetCreditLimitAsync(organization.SubscriptionPlan);
                    bool isOverDebtLimit = organization.AccruedFees >= creditLimit;

                    context.Response.OnStarting(() => {
                        context.Response.Headers.Append("X-Hedge-Usage-Current", currentMonthCount.ToString());
                        context.Response.Headers.Append("X-Hedge-Is-Trial", isUnderTrial.ToString());
                        context.Response.Headers.Append("X-Hedge-Accrued-Fees", organization.AccruedFees.ToString("F2"));
                        context.Response.Headers.Append("X-Subscription-Expired", isSubscriptionExpired.ToString());
                        return Task.CompletedTask;
                    });

                    if (context.Request.Method == HttpMethods.Post && path.Contains("/hedge"))
                    {
                        // Admins can always bypass to fix settings or pay bills
                        if (currentUser.RoleInOrganization == "Hedge_Admin")
                        {
                            await _next(context);
                            return;
                        }

                        // Rule A: Block if Subscription is expired (and not in trial)
                        if (isSubscriptionExpired && !isUnderTrial)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            await context.Response.WriteAsJsonAsync(new
                            {
                                error = "Subscription Expired",
                                message = "Your annual/monthly subscription has expired. Please renew to continue locking prices."
                            });
                            return;
                        }

                        // Rule B: Block if they owe too much money in premium fees
                        if (isOverDebtLimit)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            await context.Response.WriteAsJsonAsync(new
                            {
                                error = "Credit Limit Reached",
                                message = $"Your accrued fees ({organization.AccruedFees:C}) exceed your credit limit. Please settle your account."
                            });
                            return;
                        }

                    }
                }
            }

            await _next(context);
        }

    }
}
