using Application.Interfaces.Repositories;
using Domain.Contracts.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Infrastructure.Repositories
{
    public class BaseRepository(BuildHedgeContext context) : IBaseRepository
    {
        private readonly BuildHedgeContext _context = context ?? throw new ArgumentNullException(nameof(context));
        public async Task<T> Add<T>(T entity) where T : Domain.Contracts.Entities.BaseEntity
        {
            await _context.AddAsync<T>(entity);
            return entity;
        }

        public async Task<List<T>> Add<T>(List<T> entities) where T : BaseEntity
        {
            await _context.AddRangeAsync(entities);
            return entities;
        }

        public async Task<bool> Any<T>(Expression<Func<T, bool>> expression) where T : BaseEntity
        {
            return await _context.Set<T>().AnyAsync(expression);
        }

        public void Delete<T>(T entity) where T : Domain.Contracts.Entities.BaseEntity
        {
            _context.Set<T>().Remove(entity);

        }

        public async Task<T> Get<T>(Expression<Func<T, bool>> expression) where T : Domain.Contracts.Entities.BaseEntity
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await _context.Set<T>().Where(expression).SingleOrDefaultAsync();
#pragma warning restore CS8603 // Possible null reference return.
        }

        public async Task<IReadOnlyList<T>> GetAll<T>() where T : Domain.Contracts.Entities.BaseEntity
        {
            return await _context.Set<T>()
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<T>> GetAll<T>(Expression<Func<T, bool>> expression) where T : BaseEntity
        {
            return await _context.Set<T>()
                .Where(expression)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<T>> GetAll<T>(Expression<Func<T, bool>> expression, bool ignoreFilters = false) where T : BaseEntity
        {
            IQueryable<T> query = _context.Set<T>();

            if(ignoreFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            return await query.Where(expression).AsNoTracking().ToListAsync();
        }

        public Task<T> Get<T>(Expression<Func<T, bool>> expression, bool ignoreFilters = false) where T : BaseEntity
        {
            IQueryable<T> query = _context.Set<T>();

            if(ignoreFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            return query.FirstOrDefaultAsync(expression);
        }

        public IQueryable<T> QueryWhere<T>(Expression<Func<T, bool>> expression) where T : Domain.Contracts.Entities.BaseEntity
        {
            return _context.Set<T>()
                .Where(expression);
        }

        public async Task<T> Update<T>(T entity) where T : Domain.Contracts.Entities.BaseEntity
        {
            _context.Update(entity);
            return entity;
        }
    }
}
