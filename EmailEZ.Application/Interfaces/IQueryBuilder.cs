using EmailEZ.Domain.Common;
using System.Linq.Expressions;

namespace EmailEZ.Application.Interfaces;

/// <summary>
/// Simple query builder interface for creating dynamic queries easily.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface IQueryBuilder<TEntity> where TEntity : BaseEntity
{
    /// <summary>
    /// Adds a where condition to the query.
    /// </summary>
    /// <param name="predicate">The condition predicate.</param>
    /// <returns>The query builder for chaining.</returns>
    IQueryBuilder<TEntity> Where(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Adds an include for eager loading.
    /// </summary>
    /// <param name="include">The include expression.</param>
    /// <returns>The query builder for chaining.</returns>
    IQueryBuilder<TEntity> Include(Expression<Func<TEntity, object>> include);

    /// <summary>
    /// Adds ordering by a property.
    /// </summary>
    /// <param name="orderBy">The property to order by.</param>
    /// <returns>The query builder for chaining.</returns>
    IQueryBuilder<TEntity> OrderBy(Expression<Func<TEntity, object>> orderBy);

    /// <summary>
    /// Adds descending ordering by a property.
    /// </summary>
    /// <param name="orderBy">The property to order by descending.</param>
    /// <returns>The query builder for chaining.</returns>
    IQueryBuilder<TEntity> OrderByDescending(Expression<Func<TEntity, object>> orderBy);

    /// <summary>
    /// Sets the number of entities to skip.
    /// </summary>
    /// <param name="count">Number to skip.</param>
    /// <returns>The query builder for chaining.</returns>
    IQueryBuilder<TEntity> Skip(int count);

    /// <summary>
    /// Sets the number of entities to take.
    /// </summary>
    /// <param name="count">Number to take.</param>
    /// <returns>The query builder for chaining.</returns>
    IQueryBuilder<TEntity> Take(int count);

    /// <summary>
    /// Enables no-tracking for read-only queries.
    /// </summary>
    /// <returns>The query builder for chaining.</returns>
    IQueryBuilder<TEntity> AsNoTracking();

    /// <summary>
    /// Executes the query and returns the results.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The query results.</returns>
    Task<IEnumerable<TEntity>> ToListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the query and returns the first result or null.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The first result or null.</returns>
    Task<TEntity?> FirstOrDefaultAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the query and returns the count.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The count of entities.</returns>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the query with pagination.
    /// </summary>
    /// <param name="pageNumber">Page number (1-based).</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated results.</returns>
    Task<(IEnumerable<TEntity> Items, int TotalCount)> ToPagedListAsync(
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default);
}