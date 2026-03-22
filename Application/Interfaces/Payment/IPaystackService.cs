using Application.DTOs;
using Application.DTOs.Payment;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Payment
{
    public interface IPaystackService
    {
        Task<BaseResponse<string>> InitializeTransactionAsync(Guid billingStatementId);
        bool VerifySignature(string body, string headerSignature);
        Task<BaseResponse<bool>> ProcessWebhookAsync(PaystackEvent ev);

    }
}
