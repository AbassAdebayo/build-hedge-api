using Application.DTOs;
using Application.DTOs.Auth;
using Application.DTOs.Payment;
using Application.Interfaces.Payment;
using Application.Interfaces.Repositories;
using Domain.Contracts.Tenant;
using Domain.Entities;
using Infrastructure.Repositories;
using Infrastructure.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.X509.Qualified;
using RestSharp.Portable;
using RestSharp.Portable.HttpClient;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services.Payment
{
    public class PaystackService(ILogger<PaystackService> logger, IConfiguration config,
        IUserOrganizationMembershipRepository membership, IUserRepository userRepository,
        ITenantProvider tenantProvider, IBillingStatementRepository billingStatementRepository,
        IProcessPaymentRepository processPaymentRepository, IUnitOfWork unitOfWork,
        IOrganizationRepository organizationRepository) : IPaystackService
    {
        private readonly IConfiguration _config = config ?? throw new ArgumentNullException(nameof(config));
        private readonly ILogger<PaystackService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly IUserOrganizationMembershipRepository _membership = membership ?? throw new ArgumentNullException(nameof(membership));
        private readonly IBillingStatementRepository _billingStatementRepository = billingStatementRepository ?? throw new ArgumentNullException(nameof(billingStatementRepository));
        private readonly IProcessPaymentRepository _processPaymentRepository = processPaymentRepository ?? throw new ArgumentNullException(nameof(processPaymentRepository));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IOrganizationRepository _organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
        private readonly Guid tenantId = tenantProvider.GetTenantId();
        public async Task<BaseResponse<string>> InitializeTransactionAsync(Guid billingStatementId)
        {
            var client = new RestClient("https://api.paystack.co/transaction/initialize");
            var request = new RestRequest(Method.POST);

            request.AddHeader("Authorization", $"Bearer {_config["Paystack:SecretKey"]}");


            var membership = _membership.QueryWhere<UserOrganizationMembership>(m => m.OrganizationId == tenantId && m.RoleInOrganization == "Hedge_Admin");
            var adminId = await membership.Select(m => m.UserId).SingleOrDefaultAsync();

            var admin = await _userRepository.Get<User>(u => u.Id == adminId);
            if (admin is null) return new BaseResponse<string>("User doesn't exist", false, null);

            var statement = await _billingStatementRepository.Get<BillingStatement>(b => b.Id == billingStatementId);

            if (statement is null) return new BaseResponse<string>("Billing statement doesn't exist", false, null);


            var body = new
            {
                email = admin.Email,
                amount = (int)(statement.TotalAmountDue * 100),
                reference = $"BH_{statement.InvoiceNumber}_{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                callback_url = "https://app.buildhedge.com/billing/callback",
                metadata = new { invoice_id = statement.Id, org_id = tenantId }
            };

            request.AddJsonBody(body);
            var response = await client.Execute<PaystackInitResponse>(request);

            if (!response.IsSuccess) new BaseResponse<string>($"Paystack Init Failed: {response.Content}", false, null);
            return new BaseResponse<string>("Payment initialization successful", true, response.Data.Data.AuthorizationUrl);
        }

        public async Task<BaseResponse<bool>> ProcessWebhookAsync(PaystackEvent ev)
        {
            var reference = ev.Data.Reference;
            var paymentReferenceExists = await _processPaymentRepository.Any<ProcessedPayment>(p => p.Reference == reference);
            if (paymentReferenceExists)
            {
                _logger.LogInformation($"Duplicate webhook received for reference: {reference}");
                return new BaseResponse<bool>($"Duplicate webhook received for reference: {reference}", true, true);
            }

            // Using the execution strategy to handle potential transient failures during the transaction
            var strategy = _unitOfWork.CreateExecutionStrategy();
            BaseResponse<bool> response = await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var invoiceId = Guid.Parse(ev.Data.Metadata.InvoiceId);

                    var invoice = await _billingStatementRepository.GetBillingStatementWithOrganization(invoiceId);

                    if (invoice is null) return new BaseResponse<bool>($"Invoice {invoiceId} not found.", false, false);

                    var org = invoice.Organization;

                    // Update States
                    invoice.IsPaid = true;
                    invoice.IsPaidAt = DateTime.UtcNow;

                    // Transition from Trial to Paid
                    if (org.IsInTrial) org.IsInTrial = false;

                    // Set Expiry (Standard 30-day)
                    org.SubscriptionExpiryDate = DateTime.UtcNow.AddDays(30);
                    org.IsActive = true;

                    //await _organizationRepository.Update<Organization>(org);

                    var processPayment = new ProcessedPayment
                    {
                        Currency = ev.Data.Currency,
                        Reference = reference,
                        OrganizationId = org.Id,
                        BillingStatementId = invoice.Id,
                        AmountPaid = ev.Data.Amount / 100m
                    };

                    await _processPaymentRepository.Add<ProcessedPayment>(processPayment);

                    await _unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation($"Paystack webhook process for reference {reference} successful");
                    return new BaseResponse<bool>($"Paystack webhook process for reference {reference} successful", true, true);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, $"Failed to process Paystack webhook for reference {reference}");
                    return new BaseResponse<bool>($"Failed to process Paystack webhook for reference {reference}", false, false);
                }

            });
            return response;
        }

        public bool VerifySignature(string body, string headerSignature)
        {
            var secret = _config["Paystack:SecretKey"];
            byte[] secretBytes = Encoding.UTF8.GetBytes(secret);
            byte[] bodyBytes = Encoding.UTF8.GetBytes(body);

            using var hmac = new HMACSHA512(secretBytes);
            byte[] hash = hmac.ComputeHash(bodyBytes);
            string result = BitConverter.ToString(hash).Replace("-", "").ToLower();

            Console.WriteLine($"RAW BODY LENGTH: {body.Length}");
            Console.WriteLine($"EXPECTED HASH: {result}");
            Console.WriteLine($"RECEIVED HASH: {headerSignature}");

            return result == headerSignature;
        }
    }
}
