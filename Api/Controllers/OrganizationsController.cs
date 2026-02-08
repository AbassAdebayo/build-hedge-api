using Application.DTOs;
using Application.DTOs.Auth;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrganizationsController(IOrganizationService organizationService) : ControllerBase
    {
        private readonly IOrganizationService _organizationService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));

        [HttpPost]
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
    }
}
