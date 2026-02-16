using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Implementation
{
    public class CurrencyService(ICurrencyRepository currencyRepository) : ICurrencyService
    {
        private readonly ICurrencyRepository _currencyRepository = currencyRepository ?? throw new ArgumentNullException(nameof(currencyRepository));
        public async Task<BaseResponse<IEnumerable<Currency>>> GetAllCurrenciesAsync()
        {
            var currencies = await _currencyRepository.QueryWhere<Currency>(c => c.IsActive)
                .OrderBy(c => c.Code)
                .ToListAsync();

            if (currencies is null || !currencies.Any())
                return new BaseResponse<IEnumerable<Currency>>("No currencies found", false, null!);

            var currencyData = currencies.Select(c => new Currency
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                Symbol = c.Symbol,
            }).ToList();

            return new BaseResponse<IEnumerable<Currency>>($"{currencies.Count} Currencies retrieved successfully", true, currencyData);
        }
    }
}
