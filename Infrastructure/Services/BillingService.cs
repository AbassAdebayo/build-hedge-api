using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Contracts.MailingServices;
using Domain.Contracts.PdfHandler;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public class BillingService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BillingService> _logger;

        public BillingService(IServiceProvider serviceProvider, ILogger<BillingService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task RunBillingCycle()
        {
            using var scope = _serviceProvider.CreateScope();
            var orgRepo = scope.ServiceProvider.GetRequiredService<IOrganizationRepository>();
            var hedgeRepo = scope.ServiceProvider.GetRequiredService<IHedgeContractRepository>();
            var billingRepo = scope.ServiceProvider.GetRequiredService<IBillingRepository>();
            var globalConfig = scope.ServiceProvider.GetRequiredService<IGlobalConfigurationService>();
            var pdfService = scope.ServiceProvider.GetRequiredService<IPdfService>();
            var emailService = scope.ServiceProvider.GetRequiredService<IMailService>();
            var membershipRepo = scope.ServiceProvider.GetRequiredService<IUserOrganizationMembershipRepository>();
            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var organizations = await orgRepo.GetAll<Organization>(o => o.BusinessName != "Build Hedge", ignoreFilters: true);
            var memberships = await membershipRepo.GetAll<UserOrganizationMembership>(m => true, ignoreFilters: true);

            bool anyFailure = false;

            foreach (var org in organizations)
            {
                bool isUnderTrial = org.TrialExpiryDate.HasValue && DateTime.UtcNow <= org.TrialExpiryDate.Value;

                if (isUnderTrial && org.AccruedFees == 0) continue;

                decimal baseFee = isUnderTrial ? 0 : await globalConfig.GetBaseRateAsync(org.SubscriptionPlan);

                //var billingPeriod = DateTime.UtcNow.AddMonths(-1);
                var billingPeriod = DateTime.UtcNow;

                var startDate = new DateTime(
                    billingPeriod.Year,
                    billingPeriod.Month, 1);

                var endDate = startDate.AddMonths(1);
                var hedges = await hedgeRepo.GetHedgesForBilling(org.Id, startDate, endDate);

                 _logger.LogInformation(
                "Billing Period Start: {Start} End: {End}",
                startDate,
                endDate);

                if (hedges is null || !hedges.Any())
                {
                    _logger.LogInformation("No hedges found for {Org}", org.BusinessName);
                    continue;
                }

                var statement = new BillingStatement
                {
                    OrganizationId = org.Id,
                    InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMM}-{org.Id.ToString().Substring(0, 4)}",
                    SubscriptionBaseFee = baseFee,
                    TotalPremiumFees = hedges.Sum(h => h.PremiumFee),
                    TotalOverageFees = hedges.Sum(h => h.OverageFee),
                    DueDate = DateTime.UtcNow.AddDays(15),
                    IsPaid = false
                };

                var pdfBytes = await pdfService.GenerateInvoicePdf(org, statement, hedges);

                await billingRepo.Add(statement);

                org.AccruedFees = 0;
                await orgRepo.Update(org);

                var adminMemberships = memberships
                    .Where(m => m.OrganizationId == org.Id && m.RoleInOrganization == "Hedge_Admin")
                    .ToList();

                var adminIds = adminMemberships.Select(m => m.UserId);

                if (adminIds.Any())
                {
                    var admins = await userRepo.GetAll<User>(u => adminIds.Contains(u.Id), ignoreFilters: true);

                    foreach (var admin in admins)
                    {
                        bool sent = await emailService.SendInvoiceMail(admin.Email, org, statement, pdfBytes);
                        if (!sent) anyFailure = true;
                    }
                }
            }

            await unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                anyFailure
                ? "Billing cycle completed with failures"
                : "Billing cycle completed successfully");
        }

        public async Task SendPaymentReminders()
        {
            using var scope = _serviceProvider.CreateScope();
            var billingRepo = scope.ServiceProvider.GetRequiredService<IBillingRepository>();
            var emailService = scope.ServiceProvider.GetRequiredService<IMailService>();
            var orgRepo = scope.ServiceProvider.GetRequiredService<IOrganizationRepository>();
            var hedgeRepo = scope.ServiceProvider.GetRequiredService<IHedgeContractRepository>();
            var pdfService = scope.ServiceProvider.GetRequiredService<IPdfService>();
            var membershipRepo = scope.ServiceProvider.GetRequiredService<IUserOrganizationMembershipRepository>();
            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            var organizations = await orgRepo.GetAll<Organization>(o => o.BusinessName != "Build Hedge", ignoreFilters: true);
            var memberships = await membershipRepo.GetAll<UserOrganizationMembership>(m => true, ignoreFilters: true);

            // Find all unpaid invoices that are due in 3 days or already past due
            var threeDaysFromNow = DateTime.UtcNow.AddDays(3);
            var pendingInvoices = await billingRepo.GetAll<BillingStatement>(s => !s.IsPaid && s.DueDate <= threeDaysFromNow);

            if (pendingInvoices is not null || pendingInvoices.Any())
            {
                bool anyFailure = false;

                foreach (var invoice in pendingInvoices)
                {
                    var org = await orgRepo.Get<Organization>(org => org.Id == invoice.OrganizationId);

                    var daysUntilDue = (invoice.DueDate - DateTime.UtcNow).TotalDays;

                    if (daysUntilDue <= 3) // Start reminding 3 days before it's due
                    {
                        // 3. RE-GENERATE THE BREAKDOWN (The "Financial Aid" part)
                        // We need the hedges that were part of this specific statement

                        //var billingPeriod = DateTime.UtcNow.AddMonths(-1);
                        var billingPeriod = invoice.CreatedAtUtc;

                        var startDate = new DateTime(
                            billingPeriod.Year,
                            billingPeriod.Month,
                            1);

                        var endDate = startDate.AddMonths(1);

                        var hedges = await hedgeRepo.GetHedgesForBilling(org.Id, startDate, endDate);
                        var pdfBytes = await pdfService.GenerateInvoicePdf(org, invoice, hedges);

                        var adminMemberships = memberships.Where(m => m.OrganizationId == org.Id && m.RoleInOrganization == "Hedge_Admin").ToList();
                        var adminIds = adminMemberships.Select(m => m.UserId).ToList();

                        if (adminIds.Any())
                        {
                            var admins = await userRepo.GetAll<User>(
                                u => adminIds.Contains(u.Id),
                                ignoreFilters: true
                            );

                            foreach (var admin in admins)
                            {
                                _logger.LogInformation($"Sending payment reminder for {org.BusinessName} to Admin: {admin.Email}", org.BusinessName, admin.Email);
                                bool sent = await emailService.SendInvoiceMail(admin.Email, org, invoice, pdfBytes);
                                if (!sent) anyFailure = true;
                                _logger.LogInformation($"Sent payment reminder for {invoice.InvoiceNumber} to {org.BusinessName}");
                            }
                        }
                        else
                        {
                            _logger.LogWarning("No 'Hedge_Admin' found for Organization: {OrgId}", org.Id);
                        }
                    }
                }

                string message = anyFailure ? $"Daily payment reminder cycle completed with some failures at {DateTime.UtcNow}"
                    : $"Daily payment reminder cycle completed successfully at  {DateTime.UtcNow}";
                _logger.LogInformation(message);
            }



        }

        public async Task CleanupExpiredTrials()
        {
            using var scope = _serviceProvider.CreateScope();
            var organizationRepo = scope.ServiceProvider.GetRequiredService<IOrganizationRepository>();
            var membershipRepo = scope.ServiceProvider.GetRequiredService<IUserOrganizationMembershipRepository>();
            var organizations = await organizationRepo.GetAll<Organization>(o => true, ignoreFilters: true);
            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var emailService = scope.ServiceProvider.GetRequiredService<IMailService>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var memberships = await membershipRepo.GetAll<UserOrganizationMembership>(m => true, ignoreFilters: true);

            // 1. Find all orgs still marked 'IsInTrial' but past their 14-day window
            var expiredTrials = organizations
                .Where(o => o.IsInTrial && o.TrialExpiryDate.GetValueOrDefault(DateTime.MaxValue) < DateTime.UtcNow)
                .ToList();

            if (expiredTrials is not null || expiredTrials.Any())
            {
                bool anyFailure = false;

                foreach (var org in expiredTrials)
                {
                    // 2. Flip the bit to false
                    org.IsInTrial = false;

                    var adminMemberships = memberships.Where(m => m.OrganizationId == org.Id && m.RoleInOrganization == "Hedge_Admin").ToList();
                    var adminIds = adminMemberships.Select(m => m.UserId).ToList();

                    if (adminIds.Any())
                    {
                        var admins = await userRepo.GetAll<User>(
                            u => adminIds.Contains(u.Id),
                            ignoreFilters: true
                        );

                        foreach (var admin in admins)
                        {
                            // 3. Optional: Send a "Trial Ended" email notification
                            if (org.TrialExpiryDate.HasValue && DateTime.UtcNow > org.TrialExpiryDate.Value)
                            {
                                var isSent = await emailService.SendNotificationMail(
                                   admin.Email,
                                   org.BusinessName,
                                   "Trial Expired",
                                   "Your 14-day Enterprise trial has ended. Please settle your first invoice to continue hedging.");

                                if (!isSent) anyFailure = true;
                                _logger.LogInformation($"Trial expiry notification sent to {org.BusinessName}");
                            }


                        }


                    }
                    else
                    {
                        _logger.LogWarning("No 'Hedge_Admin' found for Organization: {OrgId}", org.Id);
                    }
                }
                string message = anyFailure ? $"Trial expiry notification cycle completed with some failures at {DateTime.UtcNow}"
                    : $"Trial expiry reminder cycle completed successfully at  {DateTime.UtcNow}";
                _logger.LogInformation(message);

                await unitOfWork.SaveChangesAsync();
            }


        }
    }
}
