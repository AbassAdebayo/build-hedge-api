using Infrastructure.Services;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Jobs
{
    public class PaymentReminderJob : IJob
    {
        private readonly BillingService _billingService;

        public PaymentReminderJob(BillingService billingService)
        {
            _billingService = billingService;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            await _billingService.SendPaymentReminders();
        }
    }
}
