using Application.DTOs;
using Application.Implementation;
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
    public class UserController(IOrganizationService organizationService) : ControllerBase
    {
        private readonly IOrganizationService _organizationService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));

        [HttpGet("organizations")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetUserOrganizations()
        {
            var userIdString = User?.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return BadRequest("Invalid userId");
            }

            var userOrganizations = await _organizationService.GetOrganizationsForUserAsync(userId);
            return Ok(userOrganizations);
        }
    }
}
