using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.Observability;

/// <summary>
/// A marker type for the VK.Blocks.Observability building block.
/// Complies with BB.02.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Marker type used for dependency resolution; contains no executable logic.")]
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKObservabilityBlock
{

    static partial void RegisterBlockCustom(IVKObservabilityBuilder builder)
    {
        var services = builder.Services;

        // Core Services
        services.TryAddTransient<IVKLogEnricher, VKApplicationEnricher>();
        services.TryAddTransient<IVKLogEnricher, VKUserContextEnricher>();
        services.TryAddTransient<IVKLogEnricher, VKTraceContextEnricher>();
        services.TryAddTransient<IVKLogContextEnricher, VKActivityLogContextEnricher>();
    }

}
