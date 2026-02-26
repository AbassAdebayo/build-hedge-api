using Application.DTOs;
using Application.DTOs.GlobalSettings;
using Application.DTOs.GlobalSettings.Validator;
using Domain.Contracts.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface IGlobalConfigurationService
    {
        public Task<BaseResponse<bool>> UpdateSettingAsync(UpdateGlobalSettingsRequestModel request);
        public Task<decimal> GetBaseRateAsync(SubscriptionPlan plan);
        public Task<int> GetHedgeQuotaAsync(SubscriptionPlan plan, DateTime trialExpiry);
        public Task<decimal> GetMonthlyRiskFactorAsync();
        public Task<decimal> GetMinimumFeeAsync();
        public Task<string> GetSystemBaseCurrency();
        public Task<decimal> GetOverageFeeAsync(SubscriptionPlan plan);
        public Task<decimal> GetCreditLimitAsync(SubscriptionPlan plan);
    }
}
