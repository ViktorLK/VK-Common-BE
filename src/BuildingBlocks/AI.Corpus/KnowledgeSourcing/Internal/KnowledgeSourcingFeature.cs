using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche;
using VK.Blocks.AI.Corpus.KnowledgeSourcing.Internal;

namespace VK.Blocks.AI.Corpus.KnowledgeSourcing.Internal;

/// <summary>
/// Hook class for registering Retrieval-related DI dependencies and validations.
/// Hooks into the source-generated [VKFeature] system.
/// </summary>
internal sealed partial class KnowledgeSourcingFeature
{
    static partial void RegisterCustom(IServiceCollection services, VKKnowledgeSourcingOptions options)
    {
        _ = options;

        services.TryAddScoped<IVKStaticKnowledgeLifecycleStore, InMemoryKnowledgeLifecycleStore>();
        services.TryAddScoped<IVKRecallKnowledgeLifecycleStore, DefaultRecallKnowledgeLifecycleStore>();

        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsycheBeforePipelineStage, KnowledgeSourcingStage>());
    }

    static partial void ValidateCustom(VKKnowledgeSourcingOptions options, List<string> failures)
    {
        if (options.DefaultTokenBudget <= 0)
        {
            failures.Add("DefaultTokenBudget must be greater than zero.");
        }
    }
}
