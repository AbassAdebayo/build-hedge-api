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
    public class UserOrganizationMembershipService(IUserOrganizationMembershipRepository membershipRepository) 
        : IUserOrganizationMembershipService
    {
        private readonly IUserOrganizationMembershipRepository _membershipRepository = membershipRepository ?? throw new ArgumentNullException(nameof(membershipRepository));
        public async Task<BaseResponse<IReadOnlyList<UserOrganizationMembership>>> GetUserOrganizationMembershipsAsync(Guid userId)
        {
            var memberships = await _membershipRepository.QueryWhere<UserOrganizationMembership>(m => m.UserId == userId)
                .Include(m => m.Organization)
                .ToListAsync();
            return new BaseResponse<IReadOnlyList<UserOrganizationMembership>>
            (
                "User organization memberships retrieved successfully.",
                true,
                memberships
            );
            
        }
    }
}
