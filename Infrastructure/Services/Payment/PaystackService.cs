using Application.DTOs;
using Application.DTOs.Auth;
using Application.DTOs.Payment;
using Application.Interfaces.Payment;
using Application.Interfaces.Repositories;
using Domain.Contracts.MailingServices;
using Domain.Contracts.PdfHandler;
using Domain.Contracts.Tenant;
using Domain.Entities;
using Infrastructure.Repositories;
using Infrastructure.Tenant;
using Microsoft.Data.SqlClient;
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
using System.Transactions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxTokenParser;

namespace Infrastructure.Services.Payment
{
    public class PaystackService(ILogger<PaystackService> logger, IConfiguration config,
        IUserOrganizationMembershipRepository membership, IUserRepository userRepository,
        ITenantProvider tenantProvider, IBillingStatementRepository billingStatementRepository,
        IProcessPaymentRepository processPaymentRepository, IUnitOfWork unitOfWork,
        IOrganizationRepository organizationRepository, IHedgeContractRepository hedgeContract,
        IPdfService pdfService, IMailService mailService) : IPaystackService
    {
        private readonly IConfiguration _config = config ?? throw new ArgumentNullException(nameof(config));
        private readonly ILogger<PaystackService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly IUserOrganizationMembershipRepository _membership = membership ?? throw new ArgumentNullException(nameof(membership));
        private readonly IBillingStatementRepository _billingStatementRepository = billingStatementRepository ?? throw new ArgumentNullException(nameof(billingStatementRepository));
        private readonly IProcessPaymentRepository _processPaymentRepository = processPaymentRepository ?? throw new ArgumentNullException(nameof(processPaymentRepository));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IOrganizationRepository _organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
        private readonly IHedgeContractRepository _hedgeContract = hedgeContract ?? throw new ArgumentNullException(nameof(hedgeContract));
        private readonly IMailService _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
        private readonly IPdfService _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
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
            var isDuplicate = await _processPaymentRepository.Any<ProcessedPayment>(p => p.Reference == reference);
            if (isDuplicate)
            {
                _logger.LogInformation($"Duplicate webhook received for reference: {reference}");
                return new BaseResponse<bool>($"Duplicate webhook received for reference: {reference}", true, true);
            }

            Organization orgToNotify = null;
            BillingStatement invoiceToNotify = null;
            BaseResponse<bool> finalResponse = null;

            var strategy = _unitOfWork.CreateExecutionStrategy();
            BaseResponse<bool> response = await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _unitOfWork.BeginTransactionAsync())
                {
                    try
                    {
                        var invoiceId = Guid.Parse(ev.Data.Metadata.InvoiceId);

                        var invoice = await _billingStatementRepository.GetBillingStatementWithOrganization(invoiceId);

                        if (invoice is null) finalResponse =  new BaseResponse<bool>($"Invoice {invoiceId} not found.", false, false);

                        if (ev.Data.Amount != (int)(invoice.TotalAmountDue * 100))
                        {
                            _logger.LogCritical("FRAUD ALERT: Amount mismatch for Invoice {Id}", invoice.Id);
                            finalResponse =  new BaseResponse<bool>("Amount mismatch", false, false);
                        }

                        var org = invoice.Organization;

                        invoice.IsPaid = true;
                        invoice.IsPaidAt = DateTime.UtcNow;

                        if (org.IsInTrial) org.IsInTrial = false;

                        // Set Expiry (Standard 30-day)
                        org.SubscriptionExpiryDate = DateTime.UtcNow.AddDays(30);
                        org.IsActive = true;

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

                        orgToNotify = org;
                        invoiceToNotify = invoice;

                        _logger.LogInformation($"Paystack webhook process for reference {reference} successful");
                        finalResponse =  new BaseResponse<bool>($"Paystack webhook process for reference {reference} successful", true, true);

                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        _logger.LogWarning("Concurrency conflict for Invoice {Id}. Checking if already settled.", invoiceToNotify.Id);

                        var entry = ex.Entries.Single(e => e.Entity is BillingStatement);
                        await entry.ReloadAsync();

                        var currentInvoice = (BillingStatement)entry.Entity;

                        if (currentInvoice.IsPaid)
                        {
                            _logger.LogInformation("Conflict resolved: Invoice {Id} was already marked as Paid by another process.", currentInvoice.Id);
                            finalResponse =  new BaseResponse<bool>("Success (Already Paid)", true, true);
                        }

                        throw;
                    }
                    catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx)
                    {
                        if (sqlEx.Number == 1205)
                        {
                            _logger.LogWarning("Deadlock detected for Ref {Ref}. Strategy will retry.", reference);
                            throw;
                        }

                        if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                        {
                            _logger.LogInformation("Duplicate Reference {Ref} detected. Skipping safely.", reference);
                            finalResponse =  new BaseResponse<bool>("Already Processed", true, true);
                        }

                        await transaction.RollbackAsync();
                        throw;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogCritical(ex, "FATAL: Payment processing failed for Ref {Ref}", ev.Data.Reference);
                        finalResponse =  new BaseResponse<bool>($"Failed to process Paystack webhook for reference {reference}", false, false);
                    }
                }

                if (finalResponse != null && finalResponse.Status && orgToNotify != null)
                {
                    try
                    {
                        await SendPaymentSuccessNotification(orgToNotify, invoiceToNotify);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Payment saved but notification failed for Ref {Ref}", reference);
                    }
                }

                return finalResponse ?? new BaseResponse<bool>("Unexpected error", false, false);
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

        private async Task SendPaymentSuccessNotification(Organization org, BillingStatement statement)
        {
            var hedges = statement.HedgesIncluded.ToList();

            foreach (var hedge in statement.HedgesIncluded)
            {
                // hedge.Material is now populated!
                var materialName = hedge.Material.Name;
                var quantity = hedge.Quantity;
                _logger.LogInformation("Processing {Material} for Invoice {Inv}", materialName, statement.InvoiceNumber);
            }

            var pdfBytes = await _pdfService.GenerateInvoicePdf(org, statement, hedges);

            // 3. Send the Email
            await _mailService.SendInvoiceMail(
                org.BillingEmail,
                org,
                statement,
                pdfBytes
            );


        }
    }
}
