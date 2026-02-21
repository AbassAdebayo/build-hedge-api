using Application.DTOs;
using Application.DTOs.GlobalSettings;
using Application.DTOs.Project;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Controllers
{
    [Authorize(Roles = "Hedge_Owner")]
    [Route("api/owner")]
    [ApiController]
    public class OwnerSettingsController(IGlobalConfigurationService globalConfigurationService,
        IPlatformRevenueService platformRevenueService) : ControllerBase
    {
        private readonly IGlobalConfigurationService _globalConfigurationService = globalConfigurationService ?? throw new ArgumentNullException(nameof(globalConfigurationService));
        private readonly IPlatformRevenueService _platformRevenueService = platformRevenueService ?? throw new ArgumentNullException(nameof(platformRevenueService));


        [Authorize(Roles = "Hedge_Admin, Hedge_Editor")]
        [HttpPost("settings")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        public async Task<IActionResult> UpdateSettings([FromBody] UpdateGlobalSettingsRequestModel request)
        {
            var updateSettings = await _globalConfigurationService.UpdateSettingAsync(request);
            return updateSettings.Status ? Ok(updateSettings) : BadRequest(updateSettings);
        }

        [Authorize(Roles = "Hedge_Admin, Hedge_Editor")]
        [HttpGet("dashboard")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        public async Task<IActionResult> Dashboard()
        {
            var dashboard = await _platformRevenueService.GetOwnerDashboardAsync();
            return dashboard.Status ? Ok(dashboard) : BadRequest(dashboard);
        }
    }
}
