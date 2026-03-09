using Application.Interfaces.Identity;
using Application.Interfaces.Services;
using Domain.Contracts.Tenant;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

public class SubscriptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;

    public SubscriptionMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var shouldBlock = false;
        object? errorResponse = null;

        using (var scope = _scopeFactory.CreateScope())
        {
            var contextFactory = scope.ServiceProvider
                .GetRequiredService<IDbContextFactory<BuildHedgeContext>>();

            await using var dbContext = await contextFactory.CreateDbContextAsync();

            var identityContext = scope.ServiceProvider.GetRequiredService<IIdentityService>();
            var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantProvider>();
            var globalConfig = scope.ServiceProvider.GetRequiredService<IGlobalConfigurationService>();

            var loggedInUser = identityContext.GetLoggedInUser();
            if (loggedInUser is not null)
            {
                var currentUserId = tenantContext.GetTenantUserId();

                var currentUser = await dbContext.Set<UserOrganizationMembership>()
                    .SingleOrDefaultAsync(m => m.UserId == currentUserId);

                if (currentUser?.RoleInOrganization != "Hedge_Owner")
                {
                    var path = context.Request.Path.Value?.ToLower() ?? "";
                    bool isPublicPath = path.Contains("/auth/") || path.Contains("/public/");

                    if (!isPublicPath)
                    {
                        var currentTenantId = tenantContext.GetTenantId();

                        var organization = await dbContext.Set<Organization>()
                            .SingleOrDefaultAsync(o => o.Id == currentTenantId);

                        if (organization != null
                            && context.Request.Method == HttpMethods.Post
                            && path.Contains("/hedges"))
                        {
                            decimal creditLimit = await globalConfig
                                .GetCreditLimitAsync(organization.SubscriptionPlan);

                            bool isUnderTrial = organization.IsInTrial
                                && organization.TrialExpiryDate.HasValue
                                && DateTime.UtcNow <= organization.TrialExpiryDate.Value;

                            bool isSubscriptionExpired = !isUnderTrial
                                && (!organization.SubscriptionExpiryDate.HasValue
                                    || DateTime.UtcNow > organization.SubscriptionExpiryDate.Value);

                            bool isOverDebtLimit = organization.AccruedFees >= creditLimit;

                            if (isSubscriptionExpired || isOverDebtLimit)
                            {
                                shouldBlock = true;
                                errorResponse = new
                                {
                                    error = "Access Denied",
                                    message = isSubscriptionExpired
                                        ? "Subscription expired."
                                        : "Credit limit reached."
                                };
                            }
                        }
                    }
                }
            }
        }

        if (shouldBlock)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await context.Response.WriteAsJsonAsync(errorResponse);
            return;
        }

        await _next(context);
    }
}