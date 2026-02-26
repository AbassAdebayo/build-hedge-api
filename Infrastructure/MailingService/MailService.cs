using Domain.Configuration;
using Domain.Contracts.MailingServices;
using Domain.Entities;
using Domain.Messaging;
using Domain.TemplateEngine;
using Infrastructure.Exceptions.Messaging;
using Infrastructure.Exceptions.TemplateEngine;
using Infrastructure.Messaging.Models;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.MailingService
{
    public class MailService(IMailSender mailSender, IRazorEngine razorEngine,
        IOptions<EmailConfiguration> options, ILogger<MailService> logger) : IMailService
    {
        private readonly IMailSender _mailSender = mailSender ?? throw new ArgumentNullException(nameof(mailSender));
        private readonly IRazorEngine _razorEngine = razorEngine ?? throw new ArgumentNullException(nameof(razorEngine));
        private readonly EmailConfiguration _emailConfiguration = options.Value ?? throw new ArgumentException(nameof(options));
        private readonly ILogger<MailService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<bool> SendChangePasswordMail(string email, string name, string userPassword)
        {
            try
            {
                var model = new ChangePassword()
                {
                    Name = name,
                    Email = email,
                };
                var mailBody = await _razorEngine.ParseAsync("ChangePasswordMail", model);
                return await _mailSender.Send(_emailConfiguration.FromEmail, _emailConfiguration.FromName, email, name, _emailConfiguration.ChangePasswordSubject, mailBody);
            }
            catch (RazorEngineException ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
            catch (MailSenderException ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public async Task<bool> SendForgotPasswordMail(string email, string name, string passwordResetLink)
        {
            try
            {
                var model = new ForgotPassword()
                {
                    Name = name,
                    Email = email,
                    PasswordResetLink = passwordResetLink
                };
                var mailBody = await _razorEngine.ParseAsync("ForgotPasswordMail", model);
                return await _mailSender.Send(_emailConfiguration.FromEmail, _emailConfiguration.FromName, email, name, _emailConfiguration.ForgotPasswordSubject, mailBody);
            }
            catch (RazorEngineException ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
            catch (MailSenderException ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public async Task<bool> SendInvitationMail(string email, string name, string token, string role)
        {
            try
            {
                var model = new SendInvitation()
                {
                    Name = name,
                    Email = email,
                    Token = token,
                    Role = role
                };
                var mailBody = await _razorEngine.ParseAsync("SendInvitationMail", model);
                return await _mailSender.Send(_emailConfiguration.FromEmail, _emailConfiguration.FromName, email, name, _emailConfiguration.InvitationSubject, mailBody);
            }
            catch (RazorEngineException ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
            catch (MailSenderException ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public async Task<bool> SendInvoiceMail(string email, Organization org, BillingStatement statement, byte[] pdfAttachment, bool isReminder = false)
        {
            try
            {
                var model = new SendInvoiceNotification()
                {
                    Name = org.BusinessName,
                    InvoiceNumber = statement.InvoiceNumber,
                    AmountDue = statement.TotalAmountDue,
                    DueDate = statement.DueDate,
                    BaseCurrency = org.BaseCurrencyCode,
                    IsReminder = isReminder,
                    PortalLink = "https://app.buildhedge.com/billing"
                };

                // 1. Parse the specific Invoice Razor Template
                var mailBody = await _razorEngine.ParseAsync("SendInvoiceMail", model);

                return await _mailSender.SendWithAttachment(
                    _emailConfiguration.FromEmail,
                    _emailConfiguration.FromName,
                    email,
                    org.BusinessName,
                    $"{(isReminder ? "REMINDER: " : "")}BuildHedge Invoice {statement.InvoiceNumber}",
                    mailBody,
                    pdfAttachment,
                    $"{statement.InvoiceNumber}.pdf"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send {(isReminder ? "reminder" : "invoice")} email: {ex.Message}");
                return false;
            }
        }


        public async Task<bool> SendNotificationMail(string email, string name, string title, string body)
        {
            try
            {
                var model = new SendNotification()
                {
                    Name = name,
                    Email = email,
                    Title = title,
                    Message = body

                };
                var mailBody = await _razorEngine.ParseAsync("SendNotificationMail", model);
                return await _mailSender.Send(_emailConfiguration.FromEmail, _emailConfiguration.FromName, email, name, title, mailBody);
            }
            catch (RazorEngineException ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
            catch (MailSenderException ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public async Task<bool> SendVerificationMail(string email, string name, string token)
        {
            try
            {
                var model = new SendVerification()
                {
                    Name = name,
                    Email = email,
                    Token = token
                };
                var mailBody = await _razorEngine.ParseAsync("SendVerificationMail", model);
                return await _mailSender.Send(_emailConfiguration.FromEmail, _emailConfiguration.FromName, email, name, _emailConfiguration.VerificationSubject, mailBody);
            }
            catch (RazorEngineException ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
            catch (MailSenderException ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }
    }
}