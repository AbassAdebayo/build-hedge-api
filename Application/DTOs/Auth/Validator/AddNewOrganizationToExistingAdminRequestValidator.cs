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
                .IsEnumName(typeof(Domain.Contracts.Enum.SubscriptionPlan), caseSensitive: false)
                .NotEmpty().WithMessage("Subscription plan is required.")
                .Must(x => Enum.IsDefined(typeof(Domain.Contracts.Enum.SubscriptionPlan), x))
                .Must(x => x.Equals("Premium", StringComparison.OrdinalIgnoreCase) ||
                          x.Equals("Basic", StringComparison.OrdinalIgnoreCase) ||
                          x.Equals("Standard", StringComparison.OrdinalIgnoreCase));

            RuleFor(x => x.TaxId)
                .MaximumLength(50).WithMessage("Tax ID cannot exceed 50 characters.");
            RuleFor(x => x.CurrencyId)
                .NotEmpty().WithMessage("Currency is required");

        }
    }
}
