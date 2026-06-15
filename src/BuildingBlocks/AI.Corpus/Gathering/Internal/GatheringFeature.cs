using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche;

namespace VK.Blocks.AI.Corpus.Gathering.Internal;

/// <summary>
/// Hook class for registering Gathering-related DI dependencies and validations.
/// Hooks into the source-generated [VKFeature] system.
/// </summary>
internal sealed partial class GatheringFeature
{
    static partial void RegisterCustom(IServiceCollection services, VKGatheringOptions options)
    {
        _ = options;

        services.TryAddScoped<IVKStaticKnowledgeLifecycleStore, InMemoryStaticKnowledgeLifecycleStore>();
        services.TryAddScoped<IVKRecallKnowledgeLifecycleStore, DefaultRecallKnowledgeLifecycleStore>();

        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsycheBeforePipelineStage, DefaultGatheringStage>());
    }

    static partial void ValidateCustom(VKGatheringOptions options, List<string> failures)
    {
        if (options.DefaultTokenBudget <= 0)
        {
            failures.Add("DefaultTokenBudget must be greater than zero.");
        }
    }
}
