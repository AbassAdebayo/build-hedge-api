using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.GlobalSettings.Validator
{
    public class UpdateGlobalSettingsRequestValidator : AbstractValidator<UpdateGlobalSettingsRequestModel>
    {
        public UpdateGlobalSettingsRequestValidator()
        {
            RuleFor(x => x.Key)
                .NotEmpty().WithMessage("Setting key is required.")
                .MaximumLength(100).WithMessage("Setting key cannot exceed 100 characters.");
            RuleFor(x => x.NewValue)
                .NotEmpty().WithMessage("New value is required.")
                .MaximumLength(500).WithMessage("New value cannot exceed 500 characters.");
        }
    }
}

