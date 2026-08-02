using VK.Blocks.AI.Corpus.Gathering.Internal;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Hook class for registering Gathering-related DI dependencies and validations.
/// Hooks into the source-generated [VKFeature] system.
/// </summary>
[VKFeature(typeof(VKAICorpusBlock), OptionsType = typeof(VKGatheringOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit)]
internal sealed partial class GatheringFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKGatheringOptions options)
    {
        _ = options;

        services.TryAddScoped<IVKStaticKnowledgeLifecycleStore, InMemoryStaticKnowledgeLifecycleStore>();
        services.TryAddScoped<IVKRecallKnowledgeLifecycleStore, DefaultRecallKnowledgeLifecycleStore>();

        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, DefaultGatheringStage>());
    }

    static partial void ValidateFeatureCustom(VKGatheringOptions options, List<string> failures)
    {
        if (options.DefaultTokenBudget <= 0)
        {
            failures.Add("DefaultTokenBudget must be greater than zero.");
        }
    }
}
