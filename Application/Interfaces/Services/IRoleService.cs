using Application.DTOs;
using Application.DTOs.Role;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface IRoleService
    {
        public Task<BaseResponse<IReadOnlyList<RoleResponse>>> GetAllRolesAsync();
    }
}
