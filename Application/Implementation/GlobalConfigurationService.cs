using Application.DTOs;
using Application.DTOs.GlobalSettings;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Contracts.Enum;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Implementation
{
    public class GlobalConfigurationService(IGlobalConfigurationRepository globalConfigurationRepository,
        IUnitOfWork unitOfWork, IConfiguration config) : IGlobalConfigurationService
    {
        private readonly IGlobalConfigurationRepository _globalConfiguration = globalConfigurationRepository ?? throw new ArgumentNullException(nameof(globalConfigurationRepository));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IConfiguration _config = config ?? throw new ArgumentNullException(nameof(config));

        public async Task<decimal> GetBaseRateAsync(SubscriptionPlan plan)
        {
            string key = $"{plan}_BaseRate";
            var settings = await _globalConfiguration.Get<GlobalSettings>(g => g.Key == key);

            if (settings is not null)
                return decimal.Parse(settings.Value);

            return _config.GetValue<decimal>("HedgeSettings:BasePremiumRate");

        }

        public async Task<int> GetHedgeQuotaAsync(SubscriptionPlan plan, DateTime trialExpiry)
        {
            bool isInTrial = trialExpiry >= DateTime.UtcNow;
            if(isInTrial) return 10;

            string key = $"{plan}_MaxHedgeQuota";
            var settings = await _globalConfiguration.Get<GlobalSettings>(g => g.Key == key);

            if (settings is not null) return int.Parse(settings.Value);

            return plan switch
            {
                SubscriptionPlan.Enterprise => 9999,
                SubscriptionPlan.Standard => 500,
                SubscriptionPlan.Basic => 100,
                _ => 0
            };

        }

        public async Task<decimal> GetMinimumFeeAsync()
        {
            string key = "MinimumFee";
            var settings = await _globalConfiguration.Get<GlobalSettings>(g => g.Key == key);

            if (settings is not null)
                return decimal.Parse(settings.Value);

            return _config.GetValue<decimal>("HedgeSettings:MinimumFee");
        }

        public async Task<decimal> GetMonthlyRiskFactorAsync()
        {
            string key = "MonthlyRiskFactor";
            var settings = await _globalConfiguration.Get<GlobalSettings>(g => g.Key == key);

            if (settings is not null)
                return decimal.Parse(settings.Value);

            return _config.GetValue<decimal>("HedgeSettings:MonthlyRiskFactor");
        }

        public async Task<decimal> GetOverageFeeAsync(SubscriptionPlan plan)
        {
            string key = $"{plan}_OverageFee";
            var settings = await _globalConfiguration.Get<GlobalSettings>(g => g.Key == key);

            if (settings is not null)
                return decimal.Parse(settings.Value);

            // Fallback defaults if DB is empty
            return plan switch
            {
                SubscriptionPlan.Enterprise => 1.50m,
                SubscriptionPlan.Standard => 2.50m,
                _ => 5.00m
            };
        }

        // Inside GlobalConfigService.cs
        public async Task<decimal> GetCreditLimitAsync(SubscriptionPlan plan)
        {
            string key = $"{plan}_CreditLimit";
            var settings = await _globalConfiguration.Get<GlobalSettings>(g => g.Key == key);

            if (settings is not null)
                return decimal.Parse(settings.Value);

            // Fallback defaults if the database entry doesn't exist yet
            return plan switch
            {
                SubscriptionPlan.Enterprise => 10000.00m,
                SubscriptionPlan.Standard => 2000.00m,
                SubscriptionPlan.Basic => 500.00m,
                _ => 100.00m
            };
        }

        public async Task<string> GetSystemBaseCurrency()
        {
            
            var baseCurrency = await _globalConfiguration.Get<GlobalSettings>(g => g.Key == "System_Base_Currency");

            if (baseCurrency is null)
                throw new NullReferenceException("No base currency configured in the system.");

            return baseCurrency!.Value;
        }

        public async Task<BaseResponse<bool>> UpdateSettingAsync(UpdateGlobalSettingsRequestModel request)
        {
            var settings = await _globalConfiguration.Get<GlobalSettings>(g => g.Key == request.Key);
            if(settings is null)
               await _globalConfiguration.Add<GlobalSettings>(new GlobalSettings
               {
                   Key = request.Key,
                   Value = request.NewValue,
                   CreatedAtUtc = DateTime.UtcNow

               });
            else
            {
               settings.Value = request.NewValue;
               settings.UpdatedAtUtc = DateTime.UtcNow;
            }

            return await _unitOfWork.SaveChangesAsync() > 0
            ? new BaseResponse<bool>("Setting updated successfully", true, true)
            : new BaseResponse<bool>("No changes made", false, false);
        }
    }
}
