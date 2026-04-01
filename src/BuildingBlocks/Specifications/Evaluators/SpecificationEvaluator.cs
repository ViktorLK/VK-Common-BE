using VK.Blocks.Specifications.Abstractions;

namespace VK.Blocks.Specifications.Evaluators;

/// <summary>
/// Main evaluator that applies specifications to an IQueryable.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public sealed class SpecificationEvaluator<T> : ISpecificationEvaluator<T> where T : class
{
    private readonly List<IEvaluator> _evaluators = new()
    {
        WhereEvaluator.Instance,
        OrderEvaluator.Instance,
        IncludeEvaluator.Instance,
        PaginationEvaluator.Instance
    };

    public IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> specification)
    {
        var query = inputQuery;

        foreach (var evaluator in _evaluators)
        {
            query = evaluator.GetQuery(query, specification);
        }

        return query;
    }
}
