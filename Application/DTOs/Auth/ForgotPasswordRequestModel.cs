using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Auth
{
    public record ForgotPasswordRequestModel(string email);
    public record ResetPasswordRequestModel(string password, string confirmPassword);
}
