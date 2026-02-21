using Application.DTOs;
using Application.DTOs.HedgeContract;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface IPlatformRevenueService
    {
        public Task<BaseResponse<PlatformSummaryResponse>> GetOwnerDashboardAsync();
    }
}
