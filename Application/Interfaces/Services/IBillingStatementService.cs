using Application.DTOs;
using Application.DTOs.Project;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface IBillingStatementService
    {
        public Task<BaseResponse<BillingStatement>> GetBillingInvoiceAsync(Guid billingId);
    }
}
