using Application.DTOs;
using Application.DTOs.HedgeContract;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface IHedgeContractService
    {
        public Task<BaseResponse<BulkHedgeResponse>> CreateProjectHedgesAsync(CreateHedgeContractRequestModel request, bool isPreview);
        public Task<BaseResponse<IEnumerable<ListOfProjectHedgesResponse>>> GetAllProjectHedges();
    }
}
