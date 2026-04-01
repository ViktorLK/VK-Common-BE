using VK.Blocks.Specifications.Abstractions;

namespace VK.Blocks.Specifications.Evaluators;

internal interface IEvaluator
{
    IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification) where T : class;
}
