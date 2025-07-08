using EmailEZ.Application.Interfaces;
using EmailEZ.Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EmailEZ.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation for CRUD operations on entities that inherit from BaseEntity.
/// </summary>
/// <typeparam name="TEntity">The entity type that inherits from BaseEntity.</typeparam>
public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly IApplicationDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly DbContext _dbContext;

    public GenericRepository(IApplicationDbContext context)
    {
        _context = context;
        _dbContext = (DbContext)context;
        _dbSet = _dbContext.Set<TEntity>();
    }

    #region Basic CRUD Operations

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().CountAsync(predicate, cancellationToken);
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var addedEntity = await _dbSet.AddAsync(entity, cancellationToken);
        return addedEntity.Entity;
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    public virtual void Update(TEntity entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void SoftDelete(TEntity entity)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTimeOffset.UtcNow;
        
        _dbSet.Update(entity);
    }

    public virtual async Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            SoftDelete(entity);
        }
    }

    public virtual void HardDelete(TEntity entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual async Task HardDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            HardDelete(entity);
        }
    }

    #endregion

    #region Pagination and Includes

    public virtual async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();
        
        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public virtual async Task<IEnumerable<TEntity>> GetWithIncludesAsync(
    Expression<Func<TEntity, bool>>? predicate = null,
    Expression<Func<TEntity, object>>[]? includes = null,
    CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (includes != null)
        {
            foreach (var include in includes)
            {

                query = query.Include(include);
            }
        }

        return await query.ToListAsync(cancellationToken);
    }

    #endregion

    #region Batch Operations

    public virtual async Task<int> BatchUpdateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TEntity>> updateExpression,
        CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet.Where(predicate).ToListAsync(cancellationToken);
        var updateFunc = updateExpression.Compile();
        
        foreach (var entity in entities)
        {
            var updatedEntity = updateFunc(entity);
            _dbContext.Entry(entity).CurrentValues.SetValues(updatedEntity);
        }

        return entities.Count();
    }

    public virtual async Task<int> BatchSoftDeleteAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet.Where(predicate).ToListAsync(cancellationToken);
        var deletedAt = DateTimeOffset.UtcNow;

        foreach (var entity in entities)
        {
            entity.IsDeleted = true;
            entity.DeletedAt = deletedAt;
        }

        return entities.Count();
    }

    public virtual async Task<int> BatchHardDeleteAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet.Where(predicate).ToListAsync(cancellationToken);
        _dbSet.RemoveRange(entities);
        return entities.Count();
    }

    public virtual void UpdateRange(IEnumerable<TEntity> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    public virtual async Task<(TEntity Entity, bool IsInserted)> UpsertAsync(
        TEntity entity,
        Expression<Func<TEntity, object>> keySelector,
        CancellationToken cancellationToken = default)
    {
        var keyFunc = keySelector.Compile();
        var keyValue = keyFunc(entity);
        
        var parameter = Expression.Parameter(typeof(TEntity), "e");
        var keyProperty = ((MemberExpression)keySelector.Body).Member.Name;
        var property = Expression.Property(parameter, keyProperty);
        var constant = Expression.Constant(keyValue);
        var equal = Expression.Equal(property, constant);
        var predicate = Expression.Lambda<Func<TEntity, bool>>(equal, parameter);

        var existingEntity = await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
        
        if (existingEntity == null)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            return (entity, true);
        }
        else
        {
            _dbContext.Entry(existingEntity).CurrentValues.SetValues(entity);
            return (existingEntity, false);
        }
    }

    #endregion

    #region Complex Queries

    public virtual async Task<IEnumerable<TEntity>> FromSqlAsync(
        string sql,
        object[]? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var query = parameters != null 
            ? _dbSet.FromSqlRaw(sql, parameters)
            : _dbSet.FromSqlRaw(sql);
            
        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAdvancedAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Expression<Func<TEntity, object>>[]? includes = null,
        int? skip = null,
        int? take = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet;

        if (filter != null)
        {
            query = query.Where(filter);
        }

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        if (skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }

        if (take.HasValue)
        {
            query = query.Take(take.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<TResult>> ProjectToAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.Select(selector).ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<TResult>> GroupByAsync<TKey, TResult>(
        Expression<Func<TEntity, TKey>> keySelector,
        Expression<Func<IGrouping<TKey, TEntity>, TResult>> resultSelector,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable().AsNoTracking();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query
            .GroupBy(keySelector)
            .Select(resultSelector)
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region Aggregation Operations

    public virtual async Task<TProperty?> MaxAsync<TProperty>(
        Expression<Func<TEntity, TProperty>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.MaxAsync(selector, cancellationToken);
    }

    public virtual async Task<TProperty?> MinAsync<TProperty>(
        Expression<Func<TEntity, TProperty>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.MinAsync(selector, cancellationToken);
    }

    public virtual async Task<decimal> SumAsync(
        Expression<Func<TEntity, decimal>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.SumAsync(selector, cancellationToken);
    }

    public virtual async Task<decimal> AverageAsync(
        Expression<Func<TEntity, decimal>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable().AsNoTracking();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.AverageAsync(selector, cancellationToken);
    }

    #endregion

    #region Utility Operations

    public virtual async Task ReloadAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Entry(entity).ReloadAsync(cancellationToken);
    }

    public virtual void Detach(TEntity entity)
    {
        _dbContext.Entry(entity).State = EntityState.Detached;
    }

    public virtual async Task<IEnumerable<TEntity>> GetAsNoTrackingAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.ToListAsync(cancellationToken);
    }

    #endregion

    #region Specification-based Queries

    public virtual async Task<IEnumerable<TEntity>> GetWithSpecificationAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(specification);
        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> GetFirstWithSpecificationAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(specification);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<int> CountWithSpecificationAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(specification, skipPaging: true);
        return await query.CountAsync(cancellationToken);
    }

    public virtual async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedWithSpecificationAsync(
        ISpecification<TEntity> specification,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(specification, skipPaging: true);
        var totalCount = await query.CountAsync(cancellationToken);

        query = ApplySpecification(specification);
        if (!specification.IsPagingEnabled)
        {
            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        var items = await query.ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Applies a specification to the queryable.
    /// </summary>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="skipPaging">Whether to skip paging (useful for count queries).</param>
    /// <returns>The queryable with specification applied.</returns>
    private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification, bool skipPaging = false)
    {
        var query = _dbSet.AsQueryable();

        // Apply criteria
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        // Apply includes
        query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));
        query = specification.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        if (specification.OrderBy != null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending != null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // Apply grouping
        if (specification.GroupBy != null)
        {
            // Note: GroupBy with specifications is complex and might need specific handling
            // This is a simplified implementation
        }

        // Apply split query if enabled
        if (specification.IsSplitQuery)
        {
            query = query.AsSplitQuery();
        }

        // Apply paging
        if (!skipPaging && specification.IsPagingEnabled)
        {
            if (specification.Skip.HasValue)
            {
                query = query.Skip(specification.Skip.Value);
            }

            if (specification.Take.HasValue)
            {
                query = query.Take(specification.Take.Value);
            }
        }

        return query;
    }

    #endregion

    #region Simple Query Builder

    public virtual IQueryable<TEntity> Query(Expression<Func<TEntity, bool>>? predicate = null)
    {
        var query = _dbSet.AsQueryable();
        
        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return query;
    }

    public async Task<IEnumerable<TResult>> ToListAsync<TResult>(IQueryable<TResult> query, CancellationToken cancellationToken = default)
    {
        return await query.ToListAsync(cancellationToken);
    }

    #endregion
}