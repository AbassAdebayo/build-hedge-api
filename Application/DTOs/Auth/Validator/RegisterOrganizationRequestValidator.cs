using Application.DTOs.Auth;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Auth.Validator
{
    public class RegisterOrganizationRequestValidator : AbstractValidator<RegisterOrganizationRequestModel>
    {
        public RegisterOrganizationRequestValidator()
        {
            RuleFor(x => x.BusinessName)
                .NotEmpty().WithMessage("Business name is required.")
                .MaximumLength(100).WithMessage("Business name must not exceed 100 characters.");

            RuleFor(x => x.SubscriptionPlan)
                .IsEnumName(typeof(Domain.Contracts.Enum.SubscriptionPlan), caseSensitive: false)
                .NotEmpty().WithMessage("Subscription plan is required.")
               .Must(x => x.Equals("Basic", StringComparison.OrdinalIgnoreCase) ||
                          x.Equals("Standard", StringComparison.OrdinalIgnoreCase) ||
                          x.Equals("Enterprise", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Subscription plan must be either 'Basic', 'Pro', or 'Enterprise'.");

            RuleFor(x => x.AdminEmail)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.")
                .Equal(x => x.AdminEmail.Trim()).WithMessage("Email cannot contain leading or trailing whitespace.");

            RuleFor(x => x.AdminFirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name must not exceed 50 characters.");

            RuleFor(x => x.AdminLastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.");

            RuleFor(x => x.AdminPhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Phone number must be a valid international phone number.");

            RuleFor(x => x.AdminPassword)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .MaximumLength(50).WithMessage("Password must not exceed 50 characters.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
                .Matches(@"[\W_]").WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.AdminConfirmPassword)
                .NotEmpty().WithMessage("Confirm password is required.")
                .Equal(x => x.AdminPassword).WithMessage("Confirm password must match the password.");
        }
    }
}
