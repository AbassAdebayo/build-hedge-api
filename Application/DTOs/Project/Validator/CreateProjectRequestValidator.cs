using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Project.Validator
{
    public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequestModel>
    {
        public CreateProjectRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Project name is required.")
                .MaximumLength(100).WithMessage("Project name cannot exceed 100 characters.");
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
            RuleFor(x => x.TotalBudget)
                .GreaterThan(0).WithMessage("Total budget must be greater than zero.");
            RuleFor(x => x.EndDate)
                .GreaterThan(DateTime.Now).WithMessage("End date must be in the future.");
        }
    }
}
