using VK.Blocks.Specifications.Abstractions;

namespace VK.Blocks.Specifications.Evaluators;

internal sealed class IncludeEvaluator : IEvaluator
{
    private IncludeEvaluator() { }

    public static IncludeEvaluator Instance { get; } = new();

    public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification) where T : class
    {
        // For vanilla IQueryable, we can only handle Expression-based includes if the provider supports them.
        // Usually, these are applied via Aggregate and a provider-specific extension method.
        // Since we are in BuildingBlocks, we don't have EF Core yet.
        // We will store them in the specification and let the final executor handle them, 
        // OR we can use reflection if the provider exposes an Include method.
        
        // For now, we just return the query as-is because vanilla LINQ doesn't have Include.
        // Infrastructure-specific evaluators would override this or we'd use a more complex mechanism.
        // However, we can still aggregate the includes if we have a way to apply them.
        
        return query;
    }
}
