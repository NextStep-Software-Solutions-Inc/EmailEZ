using System.Linq.Expressions;
using EmailEZ.Domain.Entities.Common;

namespace EmailEZ.Application.Interfaces;

/// <summary>
/// Generic repository interface for CRUD operations on entities that inherit from BaseEntity.
/// </summary>
/// <typeparam name="TEntity">The entity type that inherits from BaseEntity.</typeparam>
public interface IGenericRepository<TEntity> where TEntity : BaseEntity
{
    #region Basic CRUD Operations

    /// <summary>
    /// Gets an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The entity if found, otherwise null.</returns>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of all entities.</returns>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities based on a predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter entities.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of entities that match the predicate.</returns>
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first entity that matches the predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter entities.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The first entity that matches the predicate, or null if no match is found.</returns>
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity matches the predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter entities.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if any entity matches the predicate, otherwise false.</returns>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of entities that match the predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter entities.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The count of entities that match the predicate.</returns>
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The added entity.</returns>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple entities.
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated entity.</returns>
    void Update(TEntity entity);

    /// <summary>
    /// Soft deletes an entity by setting IsDeleted to true.
    /// </summary>
    /// <param name="entity">The entity to soft delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    void SoftDelete(TEntity entity);

    /// <summary>
    /// Soft deletes an entity by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to soft delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hard deletes an entity from the database.
    /// </summary>
    /// <param name="entity">The entity to hard delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    void HardDelete(TEntity entity);

    /// <summary>
    /// Hard deletes an entity by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to hard delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HardDeleteAsync(Guid id, CancellationToken cancellationToken = default);

    #endregion

    #region Pagination and Includes

    /// <summary>
    /// Gets entities with pagination support.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="predicate">Optional predicate to filter entities.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated result of entities.</returns>
    Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entities with optional filtering and include support for related entities.
    /// </summary>
    /// <param name="predicate">Optional filter expression.</param>
    /// <param name="includes">Optional array of include expressions for navigation properties.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of entities with related data loaded.</returns>
    Task<IEnumerable<TEntity>> GetWithIncludesAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Expression<Func<TEntity, object>>[]? includes = null,
        CancellationToken cancellationToken = default);
    #endregion

    #region Batch Operations

    /// <summary>
    /// Performs batch update on entities that match the predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter entities to update.</param>
    /// <param name="updateExpression">The update expression to apply.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of entities updated.</returns>
    Task<int> BatchUpdateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TEntity>> updateExpression,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs batch soft delete on entities that match the predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter entities to soft delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of entities soft deleted.</returns>
    Task<int> BatchSoftDeleteAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs batch hard delete on entities that match the predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter entities to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of entities deleted.</returns>
    Task<int> BatchHardDeleteAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates multiple entities efficiently.
    /// </summary>
    /// <param name="entities">The entities to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    void UpdateRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// Performs an upsert operation (insert or update based on existence).
    /// </summary>
    /// <param name="entity">The entity to upsert.</param>
    /// <param name="keySelector">Expression to select the key for existence check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The upserted entity and a boolean indicating if it was inserted (true) or updated (false).</returns>
    Task<(TEntity Entity, bool IsInserted)> UpsertAsync(
        TEntity entity,
        Expression<Func<TEntity, object>> keySelector,
        CancellationToken cancellationToken = default);

    #endregion

    #region Complex Queries

    /// <summary>
    /// Executes a raw SQL query that returns entities.
    /// </summary>
    /// <param name="sql">The SQL query.</param>
    /// <param name="parameters">Query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of entities from the query result.</returns>
    Task<IEnumerable<TEntity>> FromSqlAsync(
        string sql,
        object[]? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entities with advanced filtering, sorting, and includes.
    /// </summary>
    /// <param name="filter">Optional filter expression.</param>
    /// <param name="orderBy">Optional ordering function.</param>
    /// <param name="includes">Optional include expressions.</param>
    /// <param name="skip">Number of entities to skip.</param>
    /// <param name="take">Number of entities to take.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of entities matching the criteria.</returns>
    Task<IEnumerable<TEntity>> GetAdvancedAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Expression<Func<TEntity, object>>[]? includes = null,
        int? skip = null,
        int? take = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Projects entities to a different type using a selector.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="selector">The projection selector.</param>
    /// <param name="predicate">Optional filter predicate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of projected results.</returns>
    Task<IEnumerable<TResult>> ProjectToAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Groups entities and returns aggregated results.
    /// </summary>
    /// <typeparam name="TKey">The grouping key type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="keySelector">The grouping key selector.</param>
    /// <param name="resultSelector">The result selector for each group.</param>
    /// <param name="predicate">Optional filter predicate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of grouped results.</returns>
    Task<IEnumerable<TResult>> GroupByAsync<TKey, TResult>(
        Expression<Func<TEntity, TKey>> keySelector,
        Expression<Func<IGrouping<TKey, TEntity>, TResult>> resultSelector,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Aggregation Operations

    /// <summary>
    /// Gets the maximum value of a property.
    /// </summary>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <param name="selector">The property selector.</param>
    /// <param name="predicate">Optional filter predicate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The maximum value.</returns>
    Task<TProperty?> MaxAsync<TProperty>(
        Expression<Func<TEntity, TProperty>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the minimum value of a property.
    /// </summary>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <param name="selector">The property selector.</param>
    /// <param name="predicate">Optional filter predicate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The minimum value.</returns>
    Task<TProperty?> MinAsync<TProperty>(
        Expression<Func<TEntity, TProperty>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the sum of a numeric property.
    /// </summary>
    /// <param name="selector">The numeric property selector.</param>
    /// <param name="predicate">Optional filter predicate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The sum value.</returns>
    Task<decimal> SumAsync(
        Expression<Func<TEntity, decimal>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the average of a numeric property.
    /// </summary>
    /// <param name="selector">The numeric property selector.</param>
    /// <param name="predicate">Optional filter predicate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The average value.</returns>
    Task<decimal> AverageAsync(
        Expression<Func<TEntity, decimal>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Utility Operations

    /// <summary>
    /// Refreshes an entity from the database, discarding any local changes.
    /// </summary>
    /// <param name="entity">The entity to refresh.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ReloadAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Detaches an entity from the context.
    /// </summary>
    /// <param name="entity">The entity to detach.</param>
    void Detach(TEntity entity);

    /// <summary>
    /// Gets entities with no change tracking for read-only scenarios.
    /// </summary>
    /// <param name="predicate">Optional filter predicate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of entities with no tracking.</returns>
    Task<IEnumerable<TEntity>> GetAsNoTrackingAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Specification-based Queries

    /// <summary>
    /// Gets entities using a specification pattern.
    /// </summary>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of entities matching the specification.</returns>
    Task<IEnumerable<TEntity>> GetWithSpecificationAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single entity using a specification pattern.
    /// </summary>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The first entity matching the specification, or null.</returns>
    Task<TEntity?> GetFirstWithSpecificationAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities using a specification pattern.
    /// </summary>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The count of entities matching the specification.</returns>
    Task<int> CountWithSpecificationAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paged entities using a specification pattern.
    /// </summary>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated result of entities matching the specification.</returns>
    Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedWithSpecificationAsync(
        ISpecification<TEntity> specification,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    #endregion

    #region Simple Query Builder

    /// <summary>
    /// Creates a simple query builder for building dynamic queries with fluent API.
    /// </summary>
    /// <returns>A query builder instance.</returns>
    IQueryable<TEntity> Query(Expression<Func<TEntity, bool>>? predicate = null);

    // Get the materialized query with all applied filters, sorting, and includes.
    /// <summary>
    /// Gets the materialized query with all applied filters, sorting, and includes.
    /// </summary>
    /// <param name="query">The query to materialize.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of entities matching the query.</returns>
    Task<IEnumerable<TResult>> ToListAsync<TResult>(IQueryable<TResult> query, CancellationToken cancellationToken = default);


    #endregion
}
