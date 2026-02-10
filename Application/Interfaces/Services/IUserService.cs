using Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface IUserService
    {
        public Task<AuthResponse> InviteUserToOrganizationAsync(Guid adminUserId, AddUserToOrganizationRequestModel request);
    }
}
