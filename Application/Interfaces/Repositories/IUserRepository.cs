using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Application.Interfaces.Repositories
{
    public interface IUserRepository : IBaseRepository
    {
        
        Task<User> GetUserAndRoles(Guid userId);
    }
}
