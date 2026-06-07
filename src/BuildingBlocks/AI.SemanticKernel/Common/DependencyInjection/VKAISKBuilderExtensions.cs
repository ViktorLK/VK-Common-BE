using System;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.AI.SemanticKernel.Common.DependencyInjection;
using VK.Blocks.AI.SemanticKernel.Common.DependencyInjection.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel;

/// <summary>
/// Extension methods for configuring the Semantic Kernel building block.
/// </summary>
public static class VKAISKBuilderExtensions
{
    /// <summary>
    /// Adds Semantic Kernel Defaults (Plugins, Planners, Caching) used as fallback for all SK features.
    /// </summary>
    public static IVKAISKBuilder AddVKDefaults(
        this IVKAISKBuilder builder,
        Func<VKAISKDefaultsOptions, VKAISKDefaultsOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        return AISKDefaultsFeature.Register(builder, transform);
    }

    /// <summary>
    /// Enables native kernel caching for the Semantic Kernel building block.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The builder.</returns>
    public static IVKAISKBuilder WithKernelCaching(this IVKAISKBuilder builder)
    {
        VKGuard.NotNull(builder);
        builder.Services.Configure<VKAISKDefaultsOptions>(o => o = o with { EnableKernelCaching = true });
        return builder;
    }
}
