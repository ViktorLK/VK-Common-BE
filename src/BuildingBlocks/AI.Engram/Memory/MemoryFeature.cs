using VK.Blocks.AI.Engram.Memory.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Memory feature marker and registration hub (Persistence & Search only).
/// </summary>
[VKFeature(typeof(VKAIEngramBlock), OptionsType = typeof(VKMemoryOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit)]
internal sealed partial class MemoryFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKMemoryOptions options)
    {
        services.TryAddScoped<IVKMemoryStore, InMemoryMemoryStore>();
        services.TryAddScoped<IVKMemorySearchService, DefaultMemorySearchService>();
        services.TryAddSingleton<IVKPrefetchGatingPolicy, VK.Blocks.AI.Engram.Retrieval.Internal.AlwaysTriggerGatingPolicy>();
        services.TryAddScoped<IVKPredictiveMemoryPrefetcher, VK.Blocks.AI.Engram.Retrieval.Internal.DefaultPredictiveMemoryPrefetcher>();
        services.TryAddScoped<IVKAccessTracker, VK.Blocks.AI.Engram.Retrieval.Internal.DefaultAccessTracker>();
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKMemoryOptions options, System.Collections.Generic.List<string> failures)
    {
        VKGuard.NotNull(options);
        VKGuard.NotNull(failures);

        if (options.DefaultMinScore is < 0f or > 1f)
        {
            failures.Add("DefaultMinScore must be between 0.0 and 1.0.");
        }

        if (options.DefaultTopK <= 0)
        {
            failures.Add("DefaultTopK must be greater than zero.");
        }

        if (options.MaxMemoryEntriesToInject <= 0)
        {
            failures.Add("MaxMemoryEntriesToInject must be greater than zero.");
        }
    }
}
