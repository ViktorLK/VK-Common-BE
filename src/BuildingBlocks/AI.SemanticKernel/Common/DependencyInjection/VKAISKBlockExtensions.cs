using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.AI.SemanticKernel.Common.DependencyInjection;
using VK.Blocks.AI.SemanticKernel.Common.DependencyInjection.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel;

/// <summary>
/// Service collection extensions for the Semantic Kernel building block.
/// Public API entry point following BB.03.1 (Wrapper Pattern).
/// </summary>
public static class VKAISKBlockExtensions
{
    /// <summary>
    /// Adds the Semantic Kernel building block services using configuration.
    /// </summary>
    public static IVKAISKBuilder AddVKAISKBlock(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);
        return AISKBlockRegistration.Register(services, configuration);
    }

    /// <summary>
    /// Adds the Semantic Kernel building block services using a functional transformation.
    /// </summary>
    public static IVKAISKBuilder AddVKAISKBlock(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<VKAISKOptions, VKAISKOptions> transform)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);
        VKGuard.NotNull(transform);
        return AISKBlockRegistration.Register(services, configuration, transform);
    }
}
