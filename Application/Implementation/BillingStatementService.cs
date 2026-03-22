using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Implementation
{
    public class BillingStatementService(IBillingStatementRepository billingRepository,
        ILogger<BillingStatementService> logger) : IBillingStatementService
    {
        private readonly IBillingStatementRepository _billingRepository = billingRepository ?? throw new ArgumentNullException(nameof(billingRepository));
        private readonly ILogger<BillingStatementService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public async Task<BaseResponse<BillingStatement>> GetBillingInvoiceAsync(Guid billingId)
        {
            var invoice = await _billingRepository.Get<BillingStatement>(b => b.Id == billingId);
            if (invoice is null) return new BaseResponse<BillingStatement>("Invoice cannot be found", false, null!);

            return new BaseResponse<BillingStatement>("Invoice retrieved successfully", true, invoice);
        }
    }
}
