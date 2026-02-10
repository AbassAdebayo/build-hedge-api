using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Auth.Validator
{
    public class AddUserToOrganizationRequestValidator : AbstractValidator<AddUserToOrganizationRequestModel>
    {
        public AddUserToOrganizationRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First Name is required.")
                .MaximumLength(50).WithMessage("FirstName must not exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last Name is required.")
                .MaximumLength(50).WithMessage("LastName must not exceed 50 characters.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone Number is required.")
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Admin phone number must be a valid international phone number.");


            RuleFor(x => x.OrganizationId)
                .NotEmpty().WithMessage("Organization ID is required.")
                .Must(id => id != Guid.Empty).WithMessage("Organization ID must be a valid GUID.");

            RuleFor(x => x.RoleId)
                .NotEmpty().WithMessage("Role ID is required.")
                .Must(id => id != Guid.Empty).WithMessage("Role ID must be a valid GUID.");
        }
    }
}
