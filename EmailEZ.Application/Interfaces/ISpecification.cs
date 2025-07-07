using EmailEZ.Domain.Common;
using System.Linq.Expressions;

namespace EmailEZ.Application.Interfaces;

/// <summary>
/// Specification pattern interface for encapsulating business rules and complex queries.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface ISpecification<TEntity> where TEntity : BaseEntity
{
    /// <summary>
    /// Gets the criteria expression for filtering entities.
    /// </summary>
    Expression<Func<TEntity, bool>>? Criteria { get; }

    /// <summary>
    /// Gets the list of include expressions for eager loading.
    /// </summary>
    List<Expression<Func<TEntity, object>>> Includes { get; }

    /// <summary>
    /// Gets the list of include string expressions for eager loading.
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    /// Gets the order by expression.
    /// </summary>
    Expression<Func<TEntity, object>>? OrderBy { get; }

    /// <summary>
    /// Gets the order by descending expression.
    /// </summary>
    Expression<Func<TEntity, object>>? OrderByDescending { get; }

    /// <summary>
    /// Gets the group by expression.
    /// </summary>
    Expression<Func<TEntity, object>>? GroupBy { get; }

    /// <summary>
    /// Gets the number of entities to take.
    /// </summary>
    int? Take { get; }

    /// <summary>
    /// Gets the number of entities to skip.
    /// </summary>
    int? Skip { get; }

    /// <summary>
    /// Gets a value indicating whether change tracking is enabled.
    /// </summary>
    bool IsPagingEnabled { get; }

    /// <summary>
    /// Gets a value indicating whether split queries are enabled.
    /// </summary>
    bool IsSplitQuery { get; }
}