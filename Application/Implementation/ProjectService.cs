using Application.DTOs;
using Application.DTOs.Project;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Contracts.Tenant;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Application.Implementation
{
    public class ProjectService(IProjectRepository projectRepository, 
        IOrganizationRepository organizationRepository, IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider, ILogger<ProjectService> logger) : IProjectService
    {
        private readonly IProjectRepository _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        private readonly IOrganizationRepository _organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly ILogger<ProjectService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly Guid _tenantProvider = tenantProvider.GetTenantId();
        private readonly string _tenantUserName = tenantProvider.GetTenantUserName();
        public async Task<BaseResponse> CreateProjectAsync(CreateProjectRequestModel request)
        {
            var organization = await _organizationRepository.Get<Organization>(org => org.Id == _tenantProvider);
            if(organization is null)
            {
                _logger.LogWarning("Organization with Id {OrganizationId} not found.", _tenantProvider);
                return new BaseResponse("Organization not found.", true);
                
            } 
           
            var project = new Project
            {
                Name = request.Name,
                Description = request.Description,
                TotalBudget = request.TotalBudget,
                EstimatedCompletion = request.EndDate,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = _tenantUserName,
                OrganizationId = organization.Id
            };

            await _projectRepository.Add<Project>(project);
            return await _unitOfWork.SaveChangesAsync() > 0
                ? new BaseResponse("Project created successfully.", true)
                : new BaseResponse("Failed to create project.", false);

        }
        public async Task<BaseResponse<IEnumerable<ListOfProjectsResponse>>> GetAllProjects()
        {
            var projects = await _projectRepository.GetOrganizationProjects();

            if (projects is null || !projects.Any())
                return new BaseResponse<IEnumerable<ListOfProjectsResponse>>(
                    "No Projects found for this organization",
                    false,
                    null!
                    );
            var projectsResponse = projects.Select(p => new ListOfProjectsResponse
            (
               p.Id,
               p.Name,
               p.Description!,
               p.TotalBudget,
               p.EstimatedCompletion,
               p.RowVersion!,
               p.Organization.BusinessName

            )).ToList();

            return new BaseResponse<IEnumerable<ListOfProjectsResponse>>(
                "Projects fetched for this organization",
                true,
                projectsResponse

                );
        }

        public async Task<BaseResponse<ProjectDetailsResponse>> GetProjectDetails(Guid projectId)
        {
            var project = await _projectRepository.Get<Project>(p => p.Id == projectId);
            if (project is null)
                return new BaseResponse<ProjectDetailsResponse>("Project not found for this organization", false, null!);

            var projectDto = new ProjectDetailsResponse(
                project.Id, 
                project.Name, 
                project.Description!,
                project.TotalBudget,
                project.RowVersion!,
                project.EstimatedCompletion,
                _tenantProvider
                );

            return new BaseResponse<ProjectDetailsResponse>(
                "Project fetched successfully",
                true,
                projectDto

                );
        }

        public async Task<BaseResponse<Project>> UpdateProjectAsync(Guid id, UpdateProjectRequestModel request)
        {
            var project = await _projectRepository.Get<Project>(p => p.Id == id);
            if(project is null)
                return new BaseResponse<Project>("Project cannot be found", true, null!);

            project.Name = request.Name;
            project.Description = request.Description;
            project.TotalBudget = request.TotalBudget;
            project.EstimatedCompletion = request.EndDate;
            project.LastUpdatedBy = _tenantUserName;


            _projectRepository.UpdateProject(project, request.RowVersion);

            try
            {
                return await _unitOfWork.SaveChangesAsync() > 0 ? new BaseResponse<Project>(
                    "Project updated successfully",
                    true,
                    project
                    ) : new BaseResponse<Project>("Project update failed", false, null!);
            }
            catch(DbUpdateConcurrencyException ex)
            {
                _logger.LogError($"Update failed! {ex.Message}");
                throw new Exception("Concurrency Error: The project has been updated by someone else.");
            }
        }

        }
    }

