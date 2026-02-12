using Application.DTOs;
using Application.DTOs.Auth;
using Application.Interfaces.Services;
using Application.Tenant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController(IOrganizationService organizationService, IUserService userService,
        ITenantProvider tenantProvider) : ControllerBase
    {
        private readonly IOrganizationService _organizationService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        private readonly ITenantProvider _tenantProvider = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));

        [HttpPost("create")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        public async Task<IActionResult> CreateOrganization([FromBody] AddOrganizationToExistingAdminRequest request)
        {
            var userIdString = User?.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (!Guid.TryParse(userIdString, out Guid adminUserId))
            {
                return BadRequest("Invalid userId");
            }
            var createOrg = await _organizationService.AddExistingAdminToOrganizationAsync(adminUserId, request);
            return Ok(createOrg);
        }

        [Authorize(Roles = "Hedge_Admin")]
        [HttpPost("invite-user")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        public async Task<IActionResult> InviteBuildHedgeUserToOrganization([FromBody] AddUserToOrganizationRequestModel request)
        {
            var userIdString = User?.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (!Guid.TryParse(userIdString, out Guid adminUserId))
            {
                return BadRequest("Invalid userId");
            }

            var currentOrgId = _tenantProvider.GetTenantId();
            var inviteUser = await _userService.InviteUserToOrganizationAsync(adminUserId, currentOrgId, request);
            return Ok(inviteUser);
        }
    }
}
