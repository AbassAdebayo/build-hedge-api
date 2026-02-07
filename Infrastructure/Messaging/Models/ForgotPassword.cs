using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Messaging.Models
{
    public class ForgotPassword : Base
    {
        public string PasswordResetLink { get; set; }
    }
}
