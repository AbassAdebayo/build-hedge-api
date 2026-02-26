using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Contracts.MailingServices;
using Domain.Contracts.PdfHandler;
using Domain.Entities;
using Infrastructure.PdfHandler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NETCore.MailKit.Core;
using Org.BouncyCastle.Asn1.X509.Qualified;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.HedgeBackgroundWorker
{
    public class MonthlyBillingWorker(IServiceProvider serviceProvider, ILogger<MonthlyBillingWorker> logger) : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        private readonly ILogger<MonthlyBillingWorker> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var now = DateTime.UtcNow;

            // 1. RUN MONTHLY BILLING: On the 1st of every month
            if (now.Day == 1 && now.Hour == 1)
            {
                await RunBillingCycle();
            }

            // 2. RUN DAILY REMINDERS: Every day at 9 AM
            if (now.Hour == 9 && now.Minute == 0)
            {
                await SendPaymentReminders();
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }

        private async Task RunBillingCycle()
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

            var organizations = await orgRepo.GetAll<Organization>(o => true, ignoreFilters: true);
            var memberships = await membershipRepo.GetAll<UserOrganizationMembership>(m => true, ignoreFilters: true);

            bool anyFailure = false;

            foreach (var org in organizations)
            {
                // APPLY YOUR MIDDLEWARE LOGIC HERE
                bool isUnderTrial = DateTime.UtcNow <= org.CreatedAtUtc.AddDays(14);

                if (isUnderTrial && org.AccruedFees == 0) continue;

                // Calculate the Base Fee: If trial, it's $0. If not, fetch by Plan.
                decimal baseFee = isUnderTrial ? 0 : await globalConfig.GetBaseRateAsync(org.SubscriptionPlan);

                var lastMonth = DateTime.UtcNow.AddMonths(-1);
                var hedges = await hedgeRepo.GetHedgesForBilling(org.Id, lastMonth.Month, lastMonth.Year);

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

                // 3. Reset WIP (AccruedFees) to 0
                org.AccruedFees = 0;
                await orgRepo.Update(org);

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
                        _logger.LogInformation("Sending invoice for {OrgName} to Admin: {Email}", org.BusinessName, admin.Email);
                        var sent = await emailService.SendInvoiceMail(admin.Email, org, statement, pdfBytes);

                        if (!sent) anyFailure = true;
                    }
                }
                else
                {
                    _logger.LogWarning("No 'Hedge_Admin' found for Organization: {OrgId}", org.Id);
                }


            }
            string message = anyFailure ? $"Monthly billing cycle completed with some failures at {DateTime.UtcNow}"
                : $"Monthly billing cycle completed successfully at  {DateTime.UtcNow}";

            logger.LogError(message);



        }

        private async Task SendPaymentReminders()
        {
            using var scope = _serviceProvider.CreateScope();
            var billingRepo = scope.ServiceProvider.GetRequiredService<IBillingRepository>();
            var emailService = scope.ServiceProvider.GetRequiredService<IMailService>();
            var orgRepo = scope.ServiceProvider.GetRequiredService<IOrganizationRepository>();
            var hedgeRepo = scope.ServiceProvider.GetRequiredService<IHedgeContractRepository>();
            var pdfService = scope.ServiceProvider.GetRequiredService<IPdfService>();
            var membershipRepo = scope.ServiceProvider.GetRequiredService<IUserOrganizationMembershipRepository>();
            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            var organizations = await orgRepo.GetAll<Organization>(o => true, ignoreFilters: true);
            var memberships = await membershipRepo.GetAll<UserOrganizationMembership>(m => true, ignoreFilters: true);

            // Find all unpaid invoices that are due in 3 days or already past due
            var threeDaysFromNow = DateTime.UtcNow.AddDays(3);
            var pendingInvoices = await billingRepo.GetAll<BillingStatement>(s => !s.IsPaid && s.DueDate <= threeDaysFromNow);

            bool anyFailure = false;

            foreach (var invoice in pendingInvoices)
            {
                var org = await orgRepo.Get<Organization>(org => org.Id == invoice.OrganizationId);

                var daysUntilDue = (invoice.DueDate - DateTime.UtcNow).TotalDays;

                if (daysUntilDue <= 3) // Start reminding 3 days before it's due
                {
                    // 3. RE-GENERATE THE BREAKDOWN (The "Financial Aid" part)
                    // We need the hedges that were part of this specific statement
                    var hedges = await hedgeRepo.GetHedgesForBilling(org.Id, invoice.CreatedAtUtc.Month, invoice.CreatedAtUtc.Year);
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


}
