using VK.Blocks.Specifications.Abstractions;
using VK.Blocks.Specifications.Evaluators;

namespace VK.Blocks.Specifications.Extensions;

/// <summary>
/// Extension methods for IQueryable to apply specifications.
/// </summary>
public static class SpecificationExtensions
{
    /// <summary>
    /// Applies the specification to the query.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="query">The original query.</param>
    /// <param name="specification">The specification to apply.</param>
    /// <returns>The modified query.</returns>
    public static IQueryable<T> ApplySpecification<T>(this IQueryable<T> query, ISpecification<T> specification) where T : class
    {
        var evaluator = new SpecificationEvaluator<T>();
        return evaluator.GetQuery(query, specification);
    }
}
