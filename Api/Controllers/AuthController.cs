using Application.DTOs;
using Application.DTOs.Auth;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IOrganizationService organizationService) : ControllerBase
    {
        private readonly IOrganizationService _organizationService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));

        [HttpPost("register")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        public async Task<IActionResult> RegisterNewAccount([FromBody] RegisterOrganizationRequestModel request)
        {
            var registerOrg = await _organizationService.RegisterOrganizationAsync(request);
            return Ok(registerOrg);
        }

        
    }
}
