using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest.Enrichment.Internal; // [AP.03] Internal namespace

/// <summary>
/// Configures and registers dependencies for the Enrichment feature.
/// </summary>
internal sealed partial class EnrichmentFeature // [AP.01] sealed partial
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKEnrichmentOptions options)
    {
        _ = options;
        services.TryAddSingleton(TimeProvider.System); // [CS.06] register TimeProvider.System if not already registered, [AP.02] TryAdd
        services.TryAddSingleton<IVKChunkMetadataEnricher, DefaultChunkMetadataEnricher>(); // [AP.02] TryAdd idempotent registration
    }

    // [SG Hook]
    static partial void ValidateCustom(VKEnrichmentOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
