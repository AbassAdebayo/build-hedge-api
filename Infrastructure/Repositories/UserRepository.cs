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
    public class UserRepository(BuildHedgeContext context) : BaseRepository(context), IUserRepository
    {
        private readonly BuildHedgeContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<User> GetUserAndRoles(Guid userId)
        {
            return await _context.Set<User>()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}
