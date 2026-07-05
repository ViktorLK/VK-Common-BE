using System.Linq;

namespace VK.Blocks.Core.Specifications.Internal;

internal sealed class OrderEvaluator : IEvaluator
{
    private OrderEvaluator() { }

    public static OrderEvaluator Instance { get; } = new();

    public IQueryable<T> GetQuery<T>(IQueryable<T> query, IVKSpecification<T> VKSpecification) where T : class
    {
        if (VKSpecification.OrderBy is not null)
        {
            query = query.OrderBy(VKSpecification.OrderBy);
        }
        else if (VKSpecification.OrderByDescending is not null)
        {
            query = query.OrderByDescending(VKSpecification.OrderByDescending);
        }

        return query;
    }
}
