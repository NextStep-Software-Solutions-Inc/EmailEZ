using EmailEZ.Application.Interfaces;
using EmailEZ.Domain.Common;
using System.Linq.Expressions;

namespace EmailEZ.Application.Specifications;

/// <summary>
/// Base specification implementation with fluent API for building complex queries.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public abstract class BaseSpecification<TEntity> : ISpecification<TEntity> where TEntity : BaseEntity
{
    public Expression<Func<TEntity, bool>>? Criteria { get; private set; }
    public List<Expression<Func<TEntity, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<TEntity, object>>? OrderBy { get; private set; }
    public Expression<Func<TEntity, object>>? OrderByDescending { get; private set; }
    public Expression<Func<TEntity, object>>? GroupBy { get; private set; }
    public int? Take { get; private set; }
    public int? Skip { get; private set; }
    public bool IsPagingEnabled { get; private set; }
    public bool IsSplitQuery { get; private set; }

    protected BaseSpecification()
    {
    }

    protected BaseSpecification(Expression<Func<TEntity, bool>> criteria)
    {
        Criteria = criteria;
    }

    /// <summary>
    /// Adds a criteria expression.
    /// </summary>
    /// <param name="criteria">The criteria expression.</param>
    protected void AddCriteria(Expression<Func<TEntity, bool>> criteria)
    {
        Criteria = criteria;
    }

    /// <summary>
    /// Adds an include expression for eager loading.
    /// </summary>
    /// <param name="includeExpression">The include expression.</param>
    protected void AddInclude(Expression<Func<TEntity, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    /// <summary>
    /// Adds an include string for eager loading.
    /// </summary>
    /// <param name="includeString">The include string.</param>
    protected void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    /// <summary>
    /// Adds an order by expression.
    /// </summary>
    /// <param name="orderByExpression">The order by expression.</param>
    protected void AddOrderBy(Expression<Func<TEntity, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    /// <summary>
    /// Adds an order by descending expression.
    /// </summary>
    /// <param name="orderByDescExpression">The order by descending expression.</param>
    protected void AddOrderByDescending(Expression<Func<TEntity, object>> orderByDescExpression)
    {
        OrderByDescending = orderByDescExpression;
    }

    /// <summary>
    /// Adds a group by expression.
    /// </summary>
    /// <param name="groupByExpression">The group by expression.</param>
    protected void AddGroupBy(Expression<Func<TEntity, object>> groupByExpression)
    {
        GroupBy = groupByExpression;
    }

    /// <summary>
    /// Applies paging to the specification.
    /// </summary>
    /// <param name="skip">Number of entities to skip.</param>
    /// <param name="take">Number of entities to take.</param>
    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    /// <summary>
    /// Enables split queries for this specification.
    /// </summary>
    protected void EnableSplitQuery()
    {
        IsSplitQuery = true;
    }
}