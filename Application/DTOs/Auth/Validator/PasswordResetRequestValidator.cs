using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Auth.Validator
{
    public class PasswordResetRequestValidator : AbstractValidator<ResetPasswordRequestModel>
    {
        public PasswordResetRequestValidator()
        {
            RuleFor(x => x.password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .MaximumLength(50).WithMessage("Password must not exceed 50 characters.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
                .Matches(@"[\W_]").WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.confirmPassword)
                .NotEmpty().WithMessage("Confirm password is required.")
                .Equal(x => x.password).WithMessage("Confirm password must match the password.");
        }
    }
}
