using VK.Blocks.Specifications.Abstractions;

namespace VK.Blocks.Specifications.Evaluators;

internal sealed class PaginationEvaluator : IEvaluator
{
    private PaginationEvaluator() { }

    public static PaginationEvaluator Instance { get; } = new();

    public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification) where T : class
    {
        if (specification.IsPagingEnabled)
        {
            query = query.Skip(specification.Skip).Take(specification.Take);
        }

        return query;
    }
}
