using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Auth.Validator
{
    public class LoginRequestValidator : AbstractValidator<LoginRequestModel>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.")
                .Equal(x => x.Email.Trim()).WithMessage("Email cannot contain leading or trailing whitespace.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
                
        }
    }
}
