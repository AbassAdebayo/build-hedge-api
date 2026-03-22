using Application.DTOs;
using Application.DTOs.Auth;
using Application.Implementation;
using Application.Interfaces.Identity;
using Application.Interfaces.IDS;
using Application.Interfaces.Services;
using Domain.Entities;
using Infrastructure.IDS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net;
using System.Security.Claims;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IOrganizationService organizationService, IUserService userService,
        UserManager<User> userManager, IUserOrganizationMembershipService membershipService,
        IIdentityService identityService, ILogger<AuthController> logger, IConfiguration configuration,
        IIdsService ids
        ) : ControllerBase
    {
        private readonly IOrganizationService _organizationService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        private readonly UserManager<User> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly IIdentityService _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        private readonly ILogger<AuthController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        private readonly IUserOrganizationMembershipService _membershipService = membershipService ?? throw new ArgumentNullException(nameof(membershipService));
        private readonly IIdsService _ids = ids ?? throw new ArgumentNullException(nameof(ids));

        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        public async Task<IActionResult> RegisterNewAccount([FromBody] RegisterOrganizationRequestModel request)
        {
            var registerOrg = await _organizationService.RegisterOrganizationAsync(request);
            return Ok(registerOrg);
        }

        [AllowAnonymous]
        [HttpGet("verify-user/{token}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized, Type = typeof(BaseResponse))]
        public async Task<IActionResult> VerifyUser([FromRoute] string token)
        {
            var response = await _userService.VerifyUserAsync(token);
            return response.Status ? Ok(response) : BadRequest(response);
        }

        [AllowAnonymous]
        [EnableRateLimiting("ResendEmailPolicy")]
        [HttpPost("resend-verification")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized, Type = typeof(BaseResponse))]
        public async Task<IActionResult> ResendVerificationMail(string email)
        {
            var response = await _userService.ResendVerificationEmailAsync(email);
            return response.Status ? Ok(response) : BadRequest(response);
        }

        [HttpPost("passwordreset/{token}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized, Type = typeof(BaseResponse))]
        public async Task<IActionResult> ResetPassword([FromRoute] string token, [FromBody] ResetPasswordRequestModel request)
        {
            var response = await _userService.ResetPasswordAsync(token, request);
            return response.Status ? Ok(response) : BadRequest(response);
        }

        [HttpPost("forgotpassword")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized, Type = typeof(BaseResponse))]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestModel request)
        {
            var response = await _userService.ForgotPasswordAsync(request);
            return response.Status ? Ok(response) : BadRequest(response);
        }

        [HttpPost("login")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(LoginResponseModel))]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            var blockCheck = await _ids.CheckLoginAttemptAsync(ip);
            if (blockCheck.IsBlocked)
            {
                return StatusCode(429, new
                {
                    message = "Too many login attempts. Try again later.",
                    remainingSeconds = blockCheck.RemainingSeconds
                });
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                 await _ids.RegisterFailedAttemptAsync(ip);
                return BadRequest(new BaseResponse("Invalid login credentials", false));
            }

            var isValidPassword = await _userManager.CheckPasswordAsync(user, $"{request.Password}{user.HashSalt}");
            if (!isValidPassword)
            {
                await _ids.RegisterFailedAttemptAsync(ip);
                return BadRequest(new BaseResponse("Invalid login credentials", false));
            }

            if(!user.IsVerified) return BadRequest(new BaseResponse("Email not verified. Please verify your email before logging in.", false));

            await _ids.ResetLoginAttemptsAsync(ip);

            var memberships = await _membershipService.GetUserOrganizationMembershipsAsync(user.Id);

            if(!memberships.Data.Any())
                return BadRequest(new BaseResponse("User does not belong to any organization.", false));

            var defaultOrg = memberships.Data.FirstOrDefault();

            var token = _identityService.GenerateToken(user, defaultOrg.OrganizationId);
            var expiry = DateTimeOffset.UtcNow.AddMinutes(Convert.ToInt32(_configuration.GetValue<string>("JwtTokenSettings:TokenExpiryPeriod")));
            var tokenResponse = new LoginResponseModel(

                    "Login Successful",
                    true,
                    new LoginResponseData
                    (
                        token,
                        user.Id,
                        $"{user.FirstName} {user.LastName}",
                        user.IsVerified,
                        defaultOrg.OrganizationId,
                        memberships.Data.Select(m => new
                        {
                            m.OrganizationId,
                            m.Organization.BusinessName,
                            m.RoleInOrganization
                        }).ToList(),
                        defaultOrg.RoleInOrganization
                        

                    )
             );

            Response.Headers.Append("Token", token);
            Response.Headers.Append("TokenExpiry", expiry.ToUnixTimeMilliseconds().ToString());
            Response.Headers.Append("Access-Control-Expose-Headers", "Token,TokenExpiry");
            return Ok(tokenResponse);


        }

        [HttpPost("switch-org/{organizationId}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized, Type = typeof(BaseResponse))]
        public async Task<IActionResult> ForgotPassword(Guid organizationId)
        {
            var userIdString = User?.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return BadRequest("Invalid userId");
            }

            var response = await _userService.SwitchOrganizationAsync(userId, organizationId);
            return response.Status ? Ok(response) : BadRequest(response);
        }


    }

}

    
    

    
