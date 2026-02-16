using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories
{
    public interface IProjectRepository : IBaseRepository
    {
        Task<IEnumerable<Project>> GetOrganizationProjects();
        void UpdateProject(Project project, byte[] originalRowVersion);
    }
}
