using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Auth.Validator
{
    public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequestModel>
    {
        public ForgotPasswordRequestValidator() 
        {
            RuleFor(x => x.email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.")
                .Equal(x => x.email.Trim()).WithMessage("Email cannot contain leading or trailing whitespace.");
        }
    }
}
