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
        public Task<AuthResponse> ResendVerificationEmailAsync(string email);
        public Task<AuthResponse> ForgotPasswordAsync(ForgotPasswordRequestModel request);
        public Task<AuthResponse> ResetPasswordAsync(string token, ResetPasswordRequestModel request);
        public Task<AuthResponse<string>> SwitchOrganizationAsync(Guid userId, Guid organizationId);
    }
}
