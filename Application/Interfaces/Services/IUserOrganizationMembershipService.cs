using Application.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface IUserOrganizationMembershipService
    {
        public Task<BaseResponse<IReadOnlyList<UserOrganizationMembership>>> GetUserOrganizationMembershipsAsync(Guid userId);
    }
}
