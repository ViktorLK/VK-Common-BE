using VK.Blocks.AI.Corpus.Ingesting.Internal;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Hook class for registering Ingesting-related DI dependencies and validations.
/// Hooks into the source-generated [VKFeature] system.
/// </summary>
[VKFeature(typeof(VKAICorpusBlock), OptionsType = typeof(VKIngestingOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit)]
internal sealed partial class IngestingFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKIngestingOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKIngestingStatusStore, InMemoryIngestingStatusStore>();
        services.TryAddSingleton<IVKKnowledgeHistoryStore, InMemoryKnowledgeHistoryStore>();
        services.TryAddScoped<IVKCorpusPoisoningShield, DefaultCorpusPoisoningShield>();
        services.TryAddScoped<IVKCorpusIngestingService, DefaultCorpusIngestingService>();
    }

    static partial void ValidateFeatureCustom(VKIngestingOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
