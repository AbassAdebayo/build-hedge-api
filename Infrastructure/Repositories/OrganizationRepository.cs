using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class OrganizationRepository(BuildHedgeContext context) : BaseRepository(context), IOrganizationRepository
    {
        private readonly BuildHedgeContext _context = context ?? throw new ArgumentNullException(nameof(context));
    }
}
