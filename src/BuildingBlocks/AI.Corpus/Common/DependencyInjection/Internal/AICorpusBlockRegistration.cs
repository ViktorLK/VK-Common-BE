using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Common.DependencyInjection.Internal;

/// <summary>
/// Internal registration workflow for the AI.Corpus block.
/// Follows BB.03 registration sequence.
/// </summary>
internal static class AICorpusBlockRegistration
{
    public static IVKAICorpusBuilder Register(
        IServiceCollection services,
        IConfiguration configuration,
        Func<VKAICorpusOptions, VKAICorpusOptions>? transform = null)
    {
        AICorpusBlockBuilder builder = new(services, configuration);

        // 1. Check-Self & Prerequisite Check
        if (services.IsVKBlockRegistered<VKAICorpusBlock>())
        {
            return builder;
        }

        // 2. Options Registration
        VKAICorpusOptions options = services.AddVKBlockOptions(configuration, transform);

        // 3. Mark-Self
        services.AddVKBlockMarker<VKAICorpusBlock>();

        // 4. Options Validation

        // 5. Diagnostics & Metadata

        // 6. Feature Toggle Exit
        if (!options.Enabled)
        {
            return builder;
        }

        return builder;
    }
}
