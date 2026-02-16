using Application.Interfaces.Repositories;
using Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class CurrencyRepository(BuildHedgeContext context) : BaseRepository(context), ICurrencyRepository
    {
    }
}
