using Application.DTOs;
using Application.DTOs.Role;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Implementation
{
    public class RoleService(IRoleRepository roleRepository, ILogger<RoleService> logger) : IRoleService
    {

        private IRoleRepository _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        private ILogger<RoleService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public async Task<BaseResponse<IReadOnlyList<RoleResponse>>> GetAllRolesAsync()
        {
            var roles = await _roleRepository.GetAll<Role>();

            if(roles is  null || !roles.Any())
            {
                _logger.LogError("No roles found in the database.");
                return new BaseResponse<IReadOnlyList<RoleResponse>> ( "No roles found", false, null!);
            }

            _logger.LogInformation("Roles retrieved successfully. Count: {Count}", roles.Count());
            return new BaseResponse<IReadOnlyList<RoleResponse>>(
                "Roles retrieved successfully",
                true,
                roles.Select(r => new RoleResponse(r.Id, r.Name, r.Description

                )).ToList());
        }
    }
}
