using System.Linq;

namespace VK.Blocks.Core.Specifications.Internal;

internal sealed class WhereEvaluator : IEvaluator
{
    private WhereEvaluator() { }

    public static WhereEvaluator Instance { get; } = new();

    public IQueryable<T> GetQuery<T>(IQueryable<T> query, IVKSpecification<T> VKSpecification) where T : class
    {
        if (VKSpecification.Criteria is not null)
        {
            query = query.Where(VKSpecification.Criteria);
        }

        return query;
    }
}
