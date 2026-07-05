using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace VK.Blocks.Core;

/// <summary>
/// Abstract base class for specifications.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public abstract class VKSpecification<T> : IVKSpecification<T>
{
    protected VKSpecification(Expression<Func<T, bool>>? criteria)
    {
        Criteria = criteria;
    }

    protected VKSpecification()
    {
    }

    public virtual Expression<Func<T, bool>>? Criteria { get; private set; }

    private readonly List<Expression<Func<T, object>>> _includes = new();
    private readonly List<string> _includeStrings = new();

    public IReadOnlyList<Expression<Func<T, object>>> Includes => _includes;

    public IReadOnlyList<string> IncludeStrings => _includeStrings;

    public Expression<Func<T, object>>? OrderBy { get; private set; }

    public Expression<Func<T, object>>? OrderByDescending { get; private set; }

    public int Take { get; private set; }

    public int Skip { get; private set; }

    public bool IsPagingEnabled { get; private set; }

    protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        _includes.Add(includeExpression);
    }

    protected virtual void AddInclude(string includeString)
    {
        _includeStrings.Add(includeString);
    }

    protected virtual void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    protected virtual void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression;
    }

    private Func<T, bool>? _compiledExpression;

    protected virtual void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    /// <inheritdoc />
    public bool IsSatisfiedBy(T entity)
    {
        if (Criteria is null)
        {
            return true;
        }

        _compiledExpression ??= Criteria.Compile();
        return _compiledExpression(entity);
    }
}
