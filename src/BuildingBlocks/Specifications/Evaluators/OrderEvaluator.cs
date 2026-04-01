using VK.Blocks.Specifications.Abstractions;

namespace VK.Blocks.Specifications.Evaluators;

internal sealed class OrderEvaluator : IEvaluator
{
    private OrderEvaluator() { }

    public static OrderEvaluator Instance { get; } = new();

    public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification) where T : class
    {
        if (specification.OrderBy != null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending != null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        return query;
    }
}
