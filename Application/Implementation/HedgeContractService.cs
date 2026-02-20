using Application.DTOs;
using Application.DTOs.HedgeContract;
using Application.ExchangeRate;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Implementation
{
    public class HedgeContractService(IProjectRepository projectRepository,
        IOrganizationRepository organizationRepository,
        ICurrencyExchangeService exchangeService, ICurrencyRepository currencyRepository, 
        IHedgeContractRepository hedgeContract, IConfiguration config,
        IUnitOfWork unitOfWork) : IHedgeContractService
    {
        private readonly IProjectRepository _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        private readonly IOrganizationRepository _organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
        private readonly ICurrencyExchangeService _exchangeService = exchangeService ?? throw new ArgumentNullException(nameof(exchangeService));
        private readonly ICurrencyRepository _currencyRepository = currencyRepository ?? throw new ArgumentNullException(nameof(currencyRepository));
        private readonly IHedgeContractRepository _hedgeContract = hedgeContract ?? throw new ArgumentNullException(nameof(hedgeContract));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly decimal _baseRate = config.GetValue<decimal>("HedgeSettings:BasePremiumRate");
        private readonly decimal _monthlyRisk = config.GetValue<decimal>("HedgeSettings:MonthlyRiskFactor");

        public async Task<BaseResponse<BulkHedgeResponse>> CreateProjectHedgesAsync(CreateHedgeContractRequestModel request, bool isPreview)
        {
            var project = await _projectRepository.Get<Project>(p => p.Id == request.ProjectId);
            if (project is null)
                return new BaseResponse<BulkHedgeResponse>("Project cannot be found", false, null!);

            var organization = await _organizationRepository.Get<Organization>(o => o.Id == project.OrganizationId);
            if (organization is null)
                return new BaseResponse<BulkHedgeResponse>("No organization found for this project", false, null!);

            var responseItems = new List<HedgeItemDetail>();
            var hedgesToSave = new List<HedgeContract>();
            decimal grandTotalPremium = 0;
            decimal grandTotalValue = 0;

            int hedgesCount = 0;
            
            foreach(var item in request.MaterialsToHedge)
            {

                if(item.ExpiryDate > project.EstimatedCompletion)
                    return new BaseResponse<BulkHedgeResponse>(
                    $"Hedge for material {item.MaterialId} cannot expire after the project ends ({project.EstimatedCompletion:dd-MM-yyyy}).",
                    false, null!);

                var currency = await _currencyRepository.Get<Currency>(c => c.Id == item.CurrencyId);
                var rateResponse = await _exchangeService.GetExchangeRateAsync(currency.Code, organization.BaseCurrencyCode);

                decimal rate = rateResponse.IsSuccess ? rateResponse.Data : 1.0m;

                // Owner's Automatic Calculation
                decimal materialValue = (item.Quantity * item.LockedPrice) * rate;

                // Calculate time-based risk
                var daysToExpiry = (item.ExpiryDate - DateTime.UtcNow).TotalDays;
                var monthsOfRisk = (decimal)Math.Max(1, Math.Ceiling(daysToExpiry / 30));

                // Formula: Base Rate + (Monthly Risk * Duration)
                decimal totalPremium = materialValue * (_baseRate + (monthsOfRisk * _monthlyRisk));

                decimal totalCostWithPremium = materialValue + totalPremium;

                responseItems.Add(new HedgeItemDetail(
                    item.MaterialId,
                    totalPremium,
                    totalCostWithPremium,
                    rate
                 ));

                grandTotalPremium += totalPremium;
                grandTotalValue += materialValue;

                if(!isPreview)
                {
                    var materialValueBase = (item.Quantity * item.LockedPrice) * rate;
                    hedgesToSave.Add(new HedgeContract
                    {
                        ProjectId = project.Id,
                        MaterialId = item.MaterialId,
                        CurrencyId = item.CurrencyId,
                        Quantity = item.Quantity,
                        LockedPrice = item.LockedPrice,
                        ExchangeRateAtLock = rate,
                        ExpiryDate = item.ExpiryDate,
                        PremiumFee = totalPremium,
                        TotalValueBaseCurrency = materialValueBase,
                        Status = Domain.Contracts.Enum.ContractStatus.Active,
                        OrganizationId = organization.Id,
                    });
                }

                hedgesCount++;
            }

            var hedgesData = new BulkHedgeResponse(grandTotalValue, grandTotalPremium, responseItems);

            if (!isPreview)
            {
                await _hedgeContract.Add(hedgesToSave);
                return await _unitOfWork.SaveChangesAsync() > 0 ? new BaseResponse<BulkHedgeResponse>(
                $"{hedgesCount} hedges saved successfully", true, hedgesData)
                : new BaseResponse<BulkHedgeResponse>("Hedges couldn't be saved", false, null!);

            }

            return new BaseResponse<BulkHedgeResponse>(
                $"{hedgesCount} hedges previewed successfully", true, hedgesData);
                

        }

        public async Task<BaseResponse<IEnumerable<ListOfProjectHedgesResponse>>> GetAllProjectHedges()
        {
            var hedges = await _hedgeContract.GetAll<HedgeContract>();
            if (hedges is null || !hedges.Any())
                return new BaseResponse<IEnumerable<ListOfProjectHedgesResponse>>(
                    "No project hedges found for this organization",
                    false,
                    null!
                    );
            var hedgesData = hedges.Select(h => new ListOfProjectHedgesResponse(
                h.Id,
                h.Quantity,
                h.LockedPrice,
                h.PremiumFee,
                h.ExpiryDate,
                h.TotalValueBaseCurrency
                )).ToList();

            return new BaseResponse<IEnumerable<ListOfProjectHedgesResponse>>(
                $"{hedges.Count} project hedges retrieved",
                true,
                hedgesData
                );
        }
    }
}
