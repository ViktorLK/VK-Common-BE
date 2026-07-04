using System.Linq;

namespace VK.Blocks.Core.Specifications.Internal;

internal interface IEvaluator
{
    IQueryable<T> GetQuery<T>(IQueryable<T> query, IVKSpecification<T> VKSpecification) where T : class;
}
