using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Common.DependencyInjection.Internal;

/// <summary>
/// Internal registration workflow for the AI.Corpus block.
/// Follows BB.03 registration sequence.
/// </summary>
internal static class VKCorpusBlockRegistration
{
    public static IVKCorpusBuilder Register(
        IServiceCollection services,
        IConfiguration configuration,
        Func<VKCorpusOptions, VKCorpusOptions>? transform = null)
    {
        VKCorpusBlockBuilder builder = new(services, configuration);

        // 1. Check-Self & Prerequisite Check
        if (services.IsVKBlockRegistered<VKCorpusBlock>())
        {
            return builder;
        }

        // 2. Options Registration
        VKCorpusOptions options = services.AddVKBlockOptions(configuration, transform);

        // 3. Mark-Self
        services.AddVKBlockMarker<VKCorpusBlock>();

        // 4. Options Validation
        services.TryAddEnumerableSingleton<IValidateOptions<VKCorpusOptions>, VKCorpusOptionsValidator>();

        // 5. Diagnostics & Metadata

        // 6. Feature Toggle Exit
        if (!options.Enabled)
        {
            return builder;
        }

        return builder;
    }
}
