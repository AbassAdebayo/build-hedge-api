using Application.DTOs;
using Application.DTOs.HedgeContract;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.ExchangeRate
{
    public interface ICurrencyExchangeService
    {
        Task<ExchangeRateBaseResponse<decimal>> GetExchangeRateAsync(string fromCode, string toCode);
    }
}
