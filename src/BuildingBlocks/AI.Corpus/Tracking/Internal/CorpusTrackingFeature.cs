using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche;

using VK.Blocks.AI.Corpus.Tracking.Internal;

namespace VK.Blocks.AI.Corpus.CorpusTracking.Internal;

/// <summary>
/// Hook class for registering Tracking-related DI dependencies and validations.
/// Hooks into the source-generated [VKFeature] system.
/// </summary>
internal sealed partial class CorpusTrackingFeature
{
    static partial void RegisterCustom(IServiceCollection services, VKCorpusTrackingOptions options)
    {
        _ = options;

        services.TryAddSingleton<IVKKnowledgeUsageStore, BasicKnowledgeUsageStore>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsycheAfterPipelineStage, KnowledgeUsageRecordStage>());
    }

    static partial void ValidateCustom(VKCorpusTrackingOptions options, List<string> failures)
    {
        _ = options;
    }
}
