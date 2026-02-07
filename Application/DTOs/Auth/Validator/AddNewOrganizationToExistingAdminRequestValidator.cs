using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Auth.Validator
{
    public class AddNewOrganizationToExistingAdminRequestValidator : AbstractValidator<AddOrganizationToExistingAdminRequest>
    {
        public AddNewOrganizationToExistingAdminRequestValidator()
        {
            RuleFor(x => x.BusinessName)
                .NotEmpty().WithMessage("Business name is required.")
                .MaximumLength(100).WithMessage("Business name cannot exceed 100 characters.");

            RuleFor(x => x.SubscriptionPlan)
                .NotEmpty().WithMessage("Subscription plan is required.")
                .Must(plan => new[] { "Basic", "Standard", "Premium" }.Contains(plan))
                .WithMessage("Subscription plan must be one of the following: Basic, Standard, Premium.");

            RuleFor(x => x.TaxId)
                .MaximumLength(50).WithMessage("Tax ID cannot exceed 50 characters.");
        }
    }
}
