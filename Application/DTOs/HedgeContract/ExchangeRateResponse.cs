using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Application.DTOs.HedgeContract
{
    public record ExchangeRateResponse(
        string Result,
        [property: JsonPropertyName("conversion_rates")] Dictionary<string, decimal> ConversionRates
     );

    public record ExchangeRateBaseResponse<T>(string Message, bool IsSuccess, T Data);
}
