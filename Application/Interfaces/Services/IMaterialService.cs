using Application.DTOs;
using Application.DTOs.Material;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface IMaterialService
    {
        Task<BaseResponse<bool>> CreateMaterialAsync(CreateMaterialRequestModel request);
        Task<BaseResponse<IEnumerable<ListOfMaterialsResponse>>> GetAllMaterialsAsync();
    }
}
