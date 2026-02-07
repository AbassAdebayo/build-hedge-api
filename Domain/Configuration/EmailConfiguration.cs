using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Configuration
{
    public class EmailConfiguration
    {
        public string FromEmail { get; set; }
        public string FromName { get; set; } = "BuildHedge";
        public string ForgotPasswordSubject { get; set; } = "ForgotPassword";
        public string ChangePasswordSubject { get; set; }
        public string ResetPasswordSubject { get; set; }
        public string VerificationSubject { get; set; } = "Verification";
        public string InvitationSubject { get; set; } = "Invitation";
    }
}
