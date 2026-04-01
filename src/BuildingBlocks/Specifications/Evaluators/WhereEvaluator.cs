using VK.Blocks.Specifications.Abstractions;

namespace VK.Blocks.Specifications.Evaluators;

internal sealed class WhereEvaluator : IEvaluator
{
    private WhereEvaluator() { }

    public static WhereEvaluator Instance { get; } = new();

    public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification) where T : class
    {
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        return query;
    }
}


