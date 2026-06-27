using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest.Indexing.Internal; // [AP.03] Internal namespace

/// <summary>
/// Configures and registers dependencies for the Indexing feature.
/// </summary>
internal sealed partial class IndexingFeature // [AP.01] sealed partial
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKIndexingOptions options)
    {
        _ = options;
        services.TryAddScoped<IVKIndexingService, DefaultIndexingService>(); // [AP.02] TryAdd idempotent registration
    }

    // [SG Hook]
    static partial void ValidateCustom(VKIndexingOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
