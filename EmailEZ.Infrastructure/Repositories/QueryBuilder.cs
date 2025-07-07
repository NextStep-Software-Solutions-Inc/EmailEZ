using EmailEZ.Application.Interfaces;
using EmailEZ.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EmailEZ.Infrastructure.Repositories;

/// <summary>
/// Simple query builder implementation for creating dynamic queries easily.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class QueryBuilder<TEntity> : IQueryBuilder<TEntity> where TEntity : BaseEntity
{
    private readonly DbSet<TEntity> _dbSet;
    private IQueryable<TEntity> _query;

    public QueryBuilder(DbSet<TEntity> dbSet)
    {
        _dbSet = dbSet;
        _query = _dbSet.AsQueryable();
    }

    public IQueryBuilder<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    {
        _query = _query.Where(predicate);
        return this;
    }

    public IQueryBuilder<TEntity> Include(Expression<Func<TEntity, object>> include)
    {
        _query = _query.Include(include);
        return this;
    }

    public IQueryBuilder<TEntity> OrderBy(Expression<Func<TEntity, object>> orderBy)
    {
        _query = _query.OrderBy(orderBy);
        return this;
    }

    public IQueryBuilder<TEntity> OrderByDescending(Expression<Func<TEntity, object>> orderBy)
    {
        _query = _query.OrderByDescending(orderBy);
        return this;
    }

    public IQueryBuilder<TEntity> Skip(int count)
    {
        _query = _query.Skip(count);
        return this;
    }

    public IQueryBuilder<TEntity> Take(int count)
    {
        _query = _query.Take(count);
        return this;
    }

    public IQueryBuilder<TEntity> AsNoTracking()
    {
        _query = _query.AsNoTracking();
        return this;
    }

    public async Task<IEnumerable<TEntity>> ToListAsync(CancellationToken cancellationToken = default)
    {
        return await _query.ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        return await _query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _query.CountAsync(cancellationToken);
    }

    public async Task<(IEnumerable<TEntity> Items, int TotalCount)> ToPagedListAsync(
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var totalCount = await _query.CountAsync(cancellationToken);
        var items = await _query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}