using Infrastructure.Services.Billing;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Jobs
{
    public class TrialCleanupJob : IJob
    {
        private readonly BillingService _billingService;

        public TrialCleanupJob(BillingService billingService)
        {
            _billingService = billingService;
        }
        public async Task Execute(IJobExecutionContext context)
        {
           await _billingService.CleanupExpiredTrials();
        }
    }
}
