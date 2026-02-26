using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Contracts.MailingServices
{
    public interface IMailService
    {
        public Task<bool> SendForgotPasswordMail(string email, string name, string passwordResetLink);

        public Task<bool> SendChangePasswordMail(string email, string name, string userPassword);

        public Task<bool> SendVerificationMail(string email, string name, string token);

        public Task<bool> SendInvitationMail(string email, string name, string token, string role);
        public Task<bool> SendNotificationMail(string email, string name, string title, string body);
        public Task<bool> SendInvoiceMail(string email, Organization org, BillingStatement statement, byte[] pdfAttachment, bool isReminder = false);
        //public Task<bool> SendReminderMail(string email, string subject, BillingStatement statement);

    }
}
