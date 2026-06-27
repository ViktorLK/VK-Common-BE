using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI.Corpus.Ingesting.Internal;

/// <summary>
/// Hook class for registering Ingesting-related DI dependencies and validations.
/// Hooks into the source-generated [VKFeature] system.
/// </summary>
internal sealed partial class IngestingFeature
{
    static partial void RegisterCustom(IServiceCollection services, VKIngestingOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKIngestingStatusStore, InMemoryIngestingStatusStore>();
        services.TryAddScoped<IVKCorpusIngestingService, DefaultCorpusIngestingService>();
    }

    static partial void ValidateCustom(VKIngestingOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
