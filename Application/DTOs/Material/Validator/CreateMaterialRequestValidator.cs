using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Material.Validator
{
    public class CreateMaterialRequestValidator : AbstractValidator<CreateMaterialRequestModel>
    {
        public CreateMaterialRequestValidator()
        {
            RuleFor(x =>  x.Name)
                .NotEmpty().WithMessage("Material name is required!");
            RuleFor(x => x.TickerSymbol)
                .NotEmpty().WithMessage("Ticker symbol is required!");
            RuleFor(x => x.Unit)
                .NotEmpty().WithMessage("Unit is required!");
        }
    }
}
