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
    }
}
