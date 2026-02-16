using System;
using System.Collections.Generic;
using System.Text;

namespace Application.ExchangeRate
{
    public interface ICurrencyExchangeService
    {
        Task<decimal> GetExchangeRateAsync(string baseCurrency, string targetCurrency);
    }
}
