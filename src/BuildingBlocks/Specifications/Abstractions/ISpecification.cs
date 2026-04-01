using System.Linq.Expressions;

namespace VK.Blocks.Specifications.Abstractions;

/// <summary>
/// Defines a contract for specification pattern to encapsulate query logic.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Gets the criteria expression for the specification.
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// Gets the list of include expressions for the specification.
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// Gets the list of include strings for the specification.
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    /// Gets the order by expression for the specification.
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    /// Gets the order by descending expression for the specification.
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
}
