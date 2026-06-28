using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest.Deduplication.Internal;

/// <summary>
/// Configures and registers dependencies for the Deduplication feature.
/// </summary>
internal sealed partial class DeduplicationFeature // [AP.01] sealed partial
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKDeduplicationOptions options)
    {
        _ = options;
        services.TryAddScoped<IVKDeduplicationChecker, VectorStoreDeduplicationChecker>(); // [AP.02] TryAdd idempotent registration
    }

    // [SG Hook]
    static partial void ValidateCustom(VKDeduplicationOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
