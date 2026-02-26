using Application.DTOs.HedgeContract;
using Application.Interfaces.ExchangeRate;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Infrastructure.ExchangeRate
{
    public class CurrencyExchangeService(HttpClient httpClient, 
        IMemoryCache cache, IConfiguration config, 
        ILogger<CurrencyExchangeService> logger) : ICurrencyExchangeService
    {
        private readonly IMemoryCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly IConfiguration _config = config ?? throw new ArgumentNullException(nameof(config));
        private const string CacheKey = "Latest_Exchange_Rates";
        private readonly ILogger<CurrencyExchangeService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public async Task<ExchangeRateBaseResponse<decimal>> GetExchangeRateAsync(string fromCode, string toCode)
        {
           if(!_cache.TryGetValue(CacheKey, out Dictionary<string, decimal> rates))
           {
                string baseUrl = _config["BuildHedgeAPIs:ExchangeRate:ExchangeRateBaseUrl"]
                    ?? throw new Exception("Base URL missing in appsettings");
                string _apiKey = _config["BuildHedgeAPIs:ExchangeRate:ExchangeRateApiKey"]
                    ?? throw new Exception("API Key missing in appsettings");

    
                var url = $"{baseUrl.TrimEnd('/')}/{_apiKey}/latest/USD";
                try
                {
                    var response = await _httpClient.GetFromJsonAsync<ExchangeRateResponse>(url);

                    if (response is null || response.Result != "success")
                    {
                        _logger.LogError("External Currency API returned failure result.");
                        return new ExchangeRateBaseResponse<decimal>("External provider error.", false, decimal.Zero);
                    }

                    rates = response.ConversionRates;

                    _cache.Set(CacheKey, rates, TimeSpan.FromHours(1));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "HTTP failure fetching exchange rates.");
                    return new ExchangeRateBaseResponse<decimal>("Network error fetching rates.", false, decimal.Zero);
                }

            }

            // Calculate the cross - rate
            // Formula: (1 / RateFrom) * RateTo

            if (rates.TryGetValue(fromCode.ToUpper(), out decimal fromRate) &&
                rates.TryGetValue(toCode.ToUpper(), out decimal toRate))
            {
                var crossRate = toRate / fromRate;
                return new ExchangeRateBaseResponse<decimal>(
                    "Rate fetched successfully", true, crossRate
                 );
            }

            _logger.LogError($"Could not find exchange rate for {fromCode} or {toCode}.");
            return new ExchangeRateBaseResponse<decimal>(
                $"Could not find exchange rate for {fromCode} or {toCode}.",
                false,
                decimal.Zero
                );
        }
    }
}
