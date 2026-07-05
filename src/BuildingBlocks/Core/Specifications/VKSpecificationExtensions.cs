using System.Collections.Generic;
using System.Linq;

namespace VK.Blocks.Core;

/// <summary>
/// Extension methods to apply specifications to queries and collections.
/// </summary>
public static class VKSpecificationExtensions
{
    /// <summary>
    /// Applies the specification to the query.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="query">The original query.</param>
    /// <param name="specification">The specification to apply.</param>
    /// <returns>The modified query.</returns>
    public static IQueryable<T> ApplySpecification<T>(this IQueryable<T> query, IVKSpecification<T> specification) where T : class
    {
        var evaluator = new VKSpecificationEvaluator<T>();
        return evaluator.GetQuery(query, specification);
    }

    /// <summary>
    /// Filters an in-memory sequence of values based on a specification.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="source">The in-memory sequence.</param>
    /// <param name="specification">The specification to apply.</param>
    /// <returns>The filtered sequence.</returns>
    public static IEnumerable<T> Where<T>(this IEnumerable<T> source, IVKSpecification<T> specification)
    {
        return source.Where(specification.IsSatisfiedBy);
    }
}
