using Application.DTOs;
using Application.DTOs.Auth;
using Application.DTOs.Project;
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
        ITenantProvider tenantProvider, IProjectService projectService) : ControllerBase
    {
        private readonly IOrganizationService _organizationService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        private readonly ITenantProvider _tenantProvider = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));
        private readonly IProjectService _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));

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

        
        [HttpGet("user")]
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

        [HttpGet("all")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetAllOrganizations()
        {
            var organizations = await _organizationService.GetAllOrganizationsAsync();
            return Ok(organizations);
        }

       
        [Authorize]
        [HttpGet("{organizationId}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetOrganizationDetails([FromRoute] Guid organizationId)
        {
            var organization = await _organizationService.GetOrganizationDetailsAsync(organizationId);
            return Ok(organization);
        }

        [Authorize(Roles = "Hedge_Admin")]
        [HttpPost("create-project")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequestModel request)
        {
            var createProject = await _projectService.CreateProjectAsync(request);
            return Ok(createProject);
        }

        [Authorize(Roles = "Hedge_Admin")]
        [HttpGet("projects/{projectId}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetProject([FromRoute] Guid projectId)
        {
            var project = await _projectService.GetProjectDetails(projectId);
            return Ok(project);
        }

        [Authorize(Roles = "Hedge_Admin, Hedge_Editor")]
        [HttpGet("projects")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetAllProjects()
        {
            var projects = await _projectService.GetAllProjects();
            return Ok(projects);
        }

        [Authorize(Roles = "Hedge_Admin, Hedge_Editor")]
        [HttpPut("projects/{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        public async Task<IActionResult> UpdateProject([FromRoute]Guid id, [FromBody] UpdateProjectRequestModel request)
        {
            var updateProject = await _projectService.UpdateProjectAsync(id, request);
            return updateProject.Status ? Ok(updateProject) : BadRequest(updateProject);
        }
    }
}
