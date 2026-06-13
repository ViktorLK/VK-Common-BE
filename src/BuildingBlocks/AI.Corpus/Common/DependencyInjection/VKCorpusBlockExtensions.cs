using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.AI.Corpus.Common.DependencyInjection.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Service collection extensions for the AI.Corpus building block.
/// Public API entry point following BB.03.
/// </summary>
public static class VKCorpusBlockExtensions
{
    /// <summary>
    /// Adds the AI.Corpus building block services using configuration.
    /// </summary>
    public static IVKCorpusBuilder AddVKCorpusBlock(this IServiceCollection services, IConfiguration configuration)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);
        return VKCorpusBlockRegistration.Register(services, configuration);
    }

    /// <summary>
    /// Adds the AI.Corpus building block services using configuration and a functional transformation override.
    /// </summary>
    public static IVKCorpusBuilder AddVKCorpusBlock(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<VKCorpusOptions, VKCorpusOptions> transform)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);
        VKGuard.NotNull(transform);
        return VKCorpusBlockRegistration.Register(services, configuration, transform);
    }
}
