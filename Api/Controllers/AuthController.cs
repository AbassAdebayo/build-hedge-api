using Application.DTOs;
using Application.DTOs.Auth;
using Application.Implementation;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IOrganizationService organizationService, IUserService userService) : ControllerBase
    {
        private readonly IOrganizationService _organizationService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));

        [HttpPost("register")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        public async Task<IActionResult> RegisterNewAccount([FromBody] RegisterOrganizationRequestModel request)
        {
            var registerOrg = await _organizationService.RegisterOrganizationAsync(request);
            return Ok(registerOrg);
        }

        [HttpGet("verify-user/{token}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized, Type = typeof(BaseResponse))]
        public async Task<IActionResult> VerifyUser([FromRoute] string token)
        {
            var response = await _userService.VerifyUserAsync(token);
            return response.Status ? Ok(response) : BadRequest(response);
        }

    }

    
    
}
    
