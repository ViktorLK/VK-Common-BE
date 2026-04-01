namespace VK.Blocks.Specifications.Abstractions;

/// <summary>
/// Defines a contract for evaluating a specification against an IQueryable.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public interface ISpecificationEvaluator<T> where T : class
{
    /// <summary>
    /// Applies the specification to the input query.
    /// </summary>
    /// <param name="inputQuery">The original IQueryable.</param>
    /// <param name="specification">The specification to apply.</param>
    /// <returns>The modified IQueryable with specification applied.</returns>
    IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> specification);
}
