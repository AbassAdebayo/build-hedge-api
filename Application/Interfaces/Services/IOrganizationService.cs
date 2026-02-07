using Application.DTOs;
using Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface IOrganizationService
    {
        public Task<AuthResponse> RegisterOrganizationAsync(RegisterOrganizationRequestModel request);
        public Task<AuthResponse> AddExistingAdminToOrganizationAsync(Guid userId, AddOrganizationToExistingAdminRequest request);
    }
}
