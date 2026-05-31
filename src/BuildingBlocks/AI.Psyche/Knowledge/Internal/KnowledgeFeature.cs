using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche.Pipeline;
using VK.Blocks.Core;
namespace VK.Blocks.AI.Psyche.Knowledge.Internal;

/// <summary>
/// Knowledge feature marker and registration hub.
/// </summary>
internal sealed partial class KnowledgeFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKKnowledgeOptions options)
    {
        if (!options.Enabled)
            return;

        services.TryAddSingleton<IVKKnowledgeStore, InMemoryKnowledgeStore>();
        services.TryAddSingleton<IVKKnowledgeRenderer, DefaultKnowledgeRenderer>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKWeavingStage, DefaultKnowledgeStage>());

        // Register non-generic extractor and formatter
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IVKPromptFormatter, DefaultKnowledgeFormatter>());
    }

    // [SG Hook]
    static partial void ValidateCustom(VKKnowledgeOptions options, List<string> failures)
    {
        VKGuard.NotNull(options);
        VKGuard.NotNull(failures);

        if (options.MaxEntriesToInject < 0)
        {
            failures.Add("MaxEntriesToInject must be non-negative.");
        }

        if (options.ReservedTokens < 0)
        {
            failures.Add("ReservedTokens must be non-negative.");
        }

        if (options.SemanticThreshold is < 0 or > 1)
        {
            failures.Add("SemanticThreshold must be between 0 and 1.");
        }
    }
}
