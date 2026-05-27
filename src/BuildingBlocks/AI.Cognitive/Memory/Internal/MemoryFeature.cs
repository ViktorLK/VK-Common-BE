using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
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
        services.TryAddScoped<IVKMemoryEchoes, BasicMemoryEchoes>();
        services.TryAddScoped<IVKMemorySummarizer, DefaultMemorySummarizer>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKOrchestrationPipelineStage, DefaultMemoryPipelineStage>());

        // Default local-first in-memory engines
        services.TryAddSingleton<IVKMemoryGraph, BasicMemoryGraph>();
        services.TryAddSingleton<IVKMemoryStructured, BasicMemoryStructured>();

        // Register basic memory pruner as singleton & hosted background service (dual registration pattern)
        services.TryAddSingleton<IVKMemoryPruner, BasicMemoryPruner>();
        services.AddHostedService<BasicMemoryPruner>(sp => (BasicMemoryPruner)sp.GetRequiredService<IVKMemoryPruner>());

        // Register memory prompt extractor
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPromptExtractor, DefaultMemoryPromptExtractor>());
    }

    // [SG Hook]
    static partial void ValidateCustom(VKMemoryOptions options, System.Collections.Generic.List<string> failures)
    {
        VKGuard.NotNull(options);
        VKGuard.NotNull(failures);

        if (options.DefaultMinScore is < 0 or > 1)
        {
            failures.Add("DefaultMinScore must be between 0 and 1.");
        }

        if (options.AutomaticPruningIntervalMinutes <= 0)
        {
            failures.Add("VKMemoryOptions.AutomaticPruningIntervalMinutes must be greater than zero.");
        }
    }
}
