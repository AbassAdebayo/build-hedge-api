using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Application.Implementation
{
    public class ProjectRepository(BuildHedgeContext context) : BaseRepository(context), IProjectRepository
    {
        private readonly BuildHedgeContext _context = context ?? throw new ArgumentNullException(nameof(context));
        public async Task<IEnumerable<Project>> GetOrganizationProjects()
        {
            return await _context.Set<Project>()
                .Include(x => x.Organization)
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAtUtc)
                .ToListAsync();
        }

        public void UpdateProject(Project project, byte[] originalRowVersion)
        {
            _context.Set<Project>().Attach(project);

            _context.Entry(project).Property(p => p.RowVersion).OriginalValue = originalRowVersion;

            _context.Entry(project).State = EntityState.Modified;
        }
    }
}
