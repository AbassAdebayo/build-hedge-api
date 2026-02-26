using Application.DTOs;
using Application.Interfaces.ExchangeRate;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController(ICurrencyService currencyService,
        ICurrencyExchangeService currencyExchange) : ControllerBase
    {
        private readonly ICurrencyService _currencyService = currencyService ?? throw new ArgumentNullException(nameof(currencyService));
        private readonly ICurrencyExchangeService _currencyExchange = currencyExchange ?? throw new ArgumentNullException(nameof(currencyExchange));

        [AllowAnonymous]
        [HttpGet("all")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetAllCurrencies()
        {
            var currencies = await _currencyService.GetAllCurrenciesAsync();

            return Ok(currencies);
        }

        [HttpGet("preview-rate")]
        public async Task<IActionResult> GetPreview([FromQuery]string from, [FromQuery]string to)
        {
            var rate = await _currencyExchange.GetExchangeRateAsync(from, to);
            return Ok(new { Rate = rate, Timestamp = DateTime.UtcNow });
        }
    }
}
