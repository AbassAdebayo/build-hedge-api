using Application.DTOs;
using Application.DTOs.Project;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface IProjectService
    {
        public Task<BaseResponse> CreateProjectAsync(CreateProjectRequestModel request);
        public Task<BaseResponse<ProjectDetailsResponse>> GetProjectDetails(Guid projectId);
        public Task<BaseResponse<IEnumerable<ListOfProjectsResponse>>> GetAllProjects();
        public Task<BaseResponse<Project>> UpdateProjectAsync(Guid id, UpdateProjectRequestModel request);

    }
}
