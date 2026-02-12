using Application.DTOs;
using Application.DTOs.Auth;
using Application.DTOs.Organization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface IOrganizationService
    {
        public Task<AuthResponse> RegisterOrganizationAsync(RegisterOrganizationRequestModel request);
        public Task<AuthResponse> AddExistingAdminToOrganizationAsync(Guid userId, AddOrganizationToExistingAdminRequest request);
        public Task<BaseResponse<IReadOnlyList<OrganizationResponse>>> GetAllOrganizationsAsync();
        public Task<BaseResponse<IReadOnlyList<OrganizationResponse>>> GetOrganizationsForUserAsync(Guid userId);
        public Task<BaseResponse<OrganizationDetailsResponse>> GetOrganizationDetailsAsync(Guid organizationId);
    }
}
