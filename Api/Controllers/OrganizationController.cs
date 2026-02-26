using Application.DTOs;
using Application.DTOs.Auth;
using Application.DTOs.HedgeContract;
using Application.DTOs.Material;
using Application.DTOs.Project;
using Application.Interfaces.Services;
using Domain.Contracts.Tenant;
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
        ITenantProvider tenantProvider, IProjectService projectService,
        IMaterialService materialService, IHedgeContractService hedgeService) : ControllerBase
    {
        private readonly IOrganizationService _organizationService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        private readonly ITenantProvider _tenantProvider = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));
        private readonly IProjectService _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        private readonly IMaterialService _materialService = materialService ?? throw new ArgumentNullException(nameof(materialService));
        private readonly IHedgeContractService _hedgesService = hedgeService ?? throw new ArgumentNullException(nameof(_hedgesService));

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

        [Authorize(Roles = "Hedge_Admin")]
        [HttpPost("create-material")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        public async Task<IActionResult> CreateMaterial([FromBody] CreateMaterialRequestModel request)
        {
            var createMaterial = await _materialService.CreateMaterialAsync(request);
            return Ok(createMaterial);
        }

        [Authorize(Roles = "Hedge_Admin, Hedge_Editor")]
        [HttpGet("materials")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetAllMaterials()
        {
            var materials = await _materialService.GetAllMaterialsAsync();
            return Ok(materials);
        }


        /// <summary>
        /// Step 1: Generates a quote for multiple materials.
        /// </summary>
        [Authorize(Roles = "Hedge_Admin, Hedge_Editor")]
        [HttpPost("create-hedges/preview")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        public async Task<IActionResult> Preview([FromBody] CreateHedgeContractRequestModel request)
        {
            var createHedges = await _hedgesService.CreateProjectHedgesAsync(request, isPreview: true);
            return Ok(createHedges);
        }


        /// <summary>
        /// Step 2: Finalizes and saves the hedge contracts to the database.
        /// </summary>
        [Authorize(Roles = "Hedge_Admin, Hedge_Editor")]
        [HttpPost("create-hedges/commit")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        public async Task<IActionResult> CreateProjectHedges([FromBody] CreateHedgeContractRequestModel request)
        {
            var createHedges = await _hedgesService.CreateProjectHedgesAsync(request, isPreview: false);
            return Ok(createHedges);
        }

        [Authorize(Roles = "Hedge_Admin, Hedge_Editor")]
        [HttpGet("project-hedges")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetAllProjectHedges()
        {
            var projectHedges = await _hedgesService.GetAllProjectHedges();
            return Ok(projectHedges);
        }
    }
}
