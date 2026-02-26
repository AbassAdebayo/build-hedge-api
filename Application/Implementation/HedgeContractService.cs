using Application.DTOs;
using Application.DTOs.HedgeContract;
using Application.Interfaces.ExchangeRate;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Contracts.Enum;
using Domain.Contracts.Tenant;
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
        IHedgeContractRepository hedgeContract, IGlobalConfigurationService globalConfigService,
        IUnitOfWork unitOfWork, ITenantProvider tenantProvider) : IHedgeContractService
    {
        private readonly IProjectRepository _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        private readonly IOrganizationRepository _organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
        private readonly ICurrencyExchangeService _exchangeService = exchangeService ?? throw new ArgumentNullException(nameof(exchangeService));
        private readonly ICurrencyRepository _currencyRepository = currencyRepository ?? throw new ArgumentNullException(nameof(currencyRepository));
        private readonly IHedgeContractRepository _hedgeContract = hedgeContract ?? throw new ArgumentNullException(nameof(hedgeContract));
        private readonly IGlobalConfigurationService _globalConfigService = globalConfigService ?? throw new ArgumentNullException(nameof(globalConfigService));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly string _tenantUserName = tenantProvider.GetTenantUserName();
        private readonly Guid _tenantUserId = tenantProvider.GetTenantUserId();
        // private readonly decimal _baseRate = config.GetValue<decimal>("HedgeSettings:BasePremiumRate");
        //private readonly decimal _monthlyRisk = config.GetValue<decimal>("HedgeSettings:MonthlyRiskFactor");
        //private readonly int _minimumFee = config.GetValue<int>("HedgeSettings:MinimumFee");

        public async Task<BaseResponse<BulkHedgeResponse>> CreateProjectHedgesAsync(CreateHedgeContractRequestModel request, bool isPreview)
        {
            var project = await _projectRepository.Get<Project>(p => p.Id == request.ProjectId);
            if (project is null)
                return new BaseResponse<BulkHedgeResponse>("Project cannot be found", false, null!);

            var organization = await _organizationRepository.Get<Organization>(o => o.Id == project.OrganizationId);
            if (organization is null)
                return new BaseResponse<BulkHedgeResponse>("No organization found for this project", false, null!);

            decimal grandTotalPremium = 0;
            decimal grandTotalValue = 0;
            decimal grandTotalOverage = 0;
            var responseItems = new List<HedgeItemDetail>();
            var hedgesToSave = new List<HedgeContract>();

            int maxHedgeAllowed = await _globalConfigService.GetHedgeQuotaAsync(organization.SubscriptionPlan, organization.TrialExpiryDate!.Value);
            decimal overageFeeRate = await _globalConfigService.GetOverageFeeAsync(organization.SubscriptionPlan);
            decimal baseRate = await _globalConfigService.GetBaseRateAsync(organization.SubscriptionPlan);
            decimal minimumFee = await _globalConfigService.GetMinimumFeeAsync();
            decimal monthlyRisk = await _globalConfigService.GetMonthlyRiskFactorAsync();

            // Get current count to determine where the "Overage" starts
            int runningMonthCount = await _hedgeContract.GetMonthlyHedgeCount(organization.Id);

            foreach (var item in request.MaterialsToHedge)
            {
                if (item.ExpiryDate > project.EstimatedCompletion)
                    return new BaseResponse<BulkHedgeResponse>(
                        $"Hedge for material {item.MaterialId} expires after project ends.", false, null!);

                var currency = await _currencyRepository.Get<Currency>(c => c.Id == item.CurrencyId);
                var rateResponse = await _exchangeService.GetExchangeRateAsync(currency.Code, organization.BaseCurrencyCode);
                decimal rate = rateResponse.IsSuccess ? rateResponse.Data : 1.0m;

                decimal materialValue = (item.Quantity * item.LockedPrice) * rate;
                var monthsOfRisk = (decimal)Math.Max(1, Math.Ceiling((item.ExpiryDate - DateTime.UtcNow).TotalDays / 30));

                // Calculate the Risk Premium (2% + Time Risk)
                decimal riskPremium = materialValue * (baseRate + (monthsOfRisk * monthlyRisk));
                riskPremium = riskPremium < minimumFee ? minimumFee : riskPremium;

                decimal currentItemOverage = 0;
                if (!isPreview)
                {
                    // If we have already hit our 9,999 (or whatever the limit is), add the fee
                    if (runningMonthCount >= maxHedgeAllowed)
                    {
                        currentItemOverage = overageFeeRate;
                    }
                    runningMonthCount++;
                }

                decimal totalItemPremium = riskPremium + currentItemOverage;
                decimal totalCostWithPremium = materialValue + totalItemPremium;

                responseItems.Add(new HedgeItemDetail(item.MaterialId, totalItemPremium, totalCostWithPremium, rate));

                grandTotalPremium += riskPremium;
                grandTotalOverage += currentItemOverage;
                grandTotalValue += materialValue;

                if (!isPreview)
                {
                    hedgesToSave.Add(new HedgeContract
                    {
                        ProjectId = project.Id,
                        MaterialId = item.MaterialId,
                        CurrencyId = item.CurrencyId,
                        Quantity = item.Quantity,
                        LockedPrice = item.LockedPrice,
                        ExchangeRateAtLock = rate,
                        ExpiryDate = item.ExpiryDate,
                        PremiumFee = riskPremium,
                        OverageFee = currentItemOverage,
                        TotalValueBaseCurrency = materialValue,
                        Status = Domain.Contracts.Enum.ContractStatus.Active,
                        OrganizationId = organization.Id,
                        CreatedBy = _tenantUserName,
                        CreatedByUserId = _tenantUserId,
                    });
                }
            }

            var hedgesData = new BulkHedgeResponse(grandTotalValue, (grandTotalPremium + grandTotalOverage), responseItems);

            if (!isPreview)
            {
                // This is where the tenant "pays" via credit/invoice
                organization.AccruedFees += (grandTotalPremium + grandTotalOverage);

                await _organizationRepository.Update(organization);
                await _hedgeContract.Add(hedgesToSave);

                return await _unitOfWork.SaveChangesAsync() > 0
                    ? new BaseResponse<BulkHedgeResponse>($"{hedgesToSave.Count} hedges saved", true, hedgesData)
                    : new BaseResponse<BulkHedgeResponse>("Hedges couldn't be saved", false, null!);
            }

            return new BaseResponse<BulkHedgeResponse>($"{request.MaterialsToHedge.Count} hedges previewed", true, hedgesData);
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
