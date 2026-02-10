using Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface IUserService
    {
        public Task<AuthResponse> InviteUserToOrganizationAsync(Guid adminUserId, Guid organizationId, AddUserToOrganizationRequestModel request);
        public Task<AuthResponse> VerifyUserAsync(string token);
    }
}
