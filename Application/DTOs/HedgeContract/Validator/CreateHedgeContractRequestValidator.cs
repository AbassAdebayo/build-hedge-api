using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.HedgeContract.Validator
{
    public class CreateHedgeContractRequestValidator: AbstractValidator<CreateHedgeContractRequestModel>
    {
        public CreateHedgeContractRequestValidator()
        {
            RuleFor(x => x.ProjectId)
                .NotEmpty().WithMessage("Project Id is required");


            RuleForEach(x => x.MaterialsToHedge).ChildRules(item =>
            {
                item.RuleFor(x => x.MaterialId).NotEmpty().WithMessage("Material is required");
                item.RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero");
                item.RuleFor(x => x.LockedPrice).GreaterThan(0).WithMessage("Quantity must be greater than zero");
                item.RuleFor(x => x.ExpiryDate).GreaterThan(DateTime.UtcNow)
                .WithMessage("Expiry date must be in the future.");
            });

        }
    }
}
