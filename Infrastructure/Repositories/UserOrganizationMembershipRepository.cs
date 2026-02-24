using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Infrastructure.Repositories
{
    public class UserOrganizationMembershipRepository(BuildHedgeContext context) : BaseRepository(context), IUserOrganizationMembershipRepository
    {
        //private readonly BuildHedgeContext _context = context ?? throw new ArgumentNullException(nameof(context));

        
    }
}
