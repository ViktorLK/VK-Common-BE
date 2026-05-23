using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Memory.Internal;

/// <summary>
/// Memory feature marker and registration hub.
/// </summary>
internal sealed partial class MemoryFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKMemoryOptions options)
    {
        services.TryAddScoped<IVKMemoryLedger, BasicMemoryLedger>();
        services.TryAddScoped<IVKMemoryEchoes, VectorMemoryEchoes>();
        services.TryAddScoped<IVKMemorySummarizer, DefaultMemorySummarizer>();

        // Default local-first in-memory engines
        services.TryAddSingleton<IVKMemoryGraph, BasicMemoryGraph>();
        services.TryAddSingleton<IVKMemoryStructured, BasicMemoryStructured>();

        // Register basic memory pruner as singleton & hosted background service (dual registration pattern)
        services.TryAddSingleton<IVKMemoryPruner, BasicMemoryPruner>();
        services.AddHostedService<BasicMemoryPruner>(sp => (BasicMemoryPruner)sp.GetRequiredService<IVKMemoryPruner>());
    }

    // [SG Hook]
    static partial void ValidateCustom(VKMemoryOptions options, System.Collections.Generic.List<string> failures)
    {
        VKGuard.NotNull(options);
        VKGuard.NotNull(failures);

        if (options.AutomaticPruningIntervalMinutes <= 0)
        {
            failures.Add("VKMemoryOptions.AutomaticPruningIntervalMinutes must be greater than zero.");
        }
    }
}
