using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace VK.Blocks.Core;

/// <summary>
/// Defines a contract for VKSpecification pattern to encapsulate query logic.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public interface IVKSpecification<T>
{
    /// <summary>
    /// Gets the criteria expression for the VKSpecification.
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// Gets the list of include expressions for the VKSpecification.
    /// </summary>
    IReadOnlyList<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// Gets the list of include strings for the VKSpecification.
    /// </summary>
    IReadOnlyList<string> IncludeStrings { get; }

    /// <summary>
    /// Gets the order by expression for the VKSpecification.
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    /// Gets the order by descending expression for the VKSpecification.
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>
    /// Gets the number of items to take for paging.
    /// </summary>
    int Take { get; }

    /// <summary>
    /// Gets the number of items to skip for paging.
    /// </summary>
    int Skip { get; }

    /// <summary>
    /// Gets a value indicating whether paging is enabled.
    /// </summary>
    bool IsPagingEnabled { get; }

    /// <summary>
    /// Evaluates if the specification is satisfied by the specified entity in memory.
    /// </summary>
    /// <param name="entity">The entity to evaluate.</param>
    /// <returns><c>true</c> if the entity satisfies the specification; otherwise, <c>false</c>.</returns>
    bool IsSatisfiedBy(T entity);
}
