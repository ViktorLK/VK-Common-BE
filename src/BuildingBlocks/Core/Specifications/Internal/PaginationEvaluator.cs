using System.Linq;

namespace VK.Blocks.Core.Specifications.Internal;

internal sealed class PaginationEvaluator : IEvaluator
{
    private PaginationEvaluator() { }

    public static PaginationEvaluator Instance { get; } = new();

    public IQueryable<T> GetQuery<T>(IQueryable<T> query, IVKSpecification<T> VKSpecification) where T : class
    {
        if (VKSpecification.IsPagingEnabled)
        {
            query = query.Skip(VKSpecification.Skip).Take(VKSpecification.Take);
        }

        return query;
    }
}
