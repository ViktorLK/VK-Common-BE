using System;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel;

/// <summary>
/// Extension methods for configuring the Semantic Kernel building block.
/// </summary>
public static partial class VKAISemanticKernelBuilderExtensions
{
    /// <summary>
    /// Enables native kernel caching for the Semantic Kernel building block.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The builder.</returns>
    public static IVKAISemanticKernelBuilder WithKernelCaching(this IVKAISemanticKernelBuilder builder)
    {
        VKGuard.NotNull(builder);
        builder.Services.Configure<VKAISemanticKernelOptions>(o => o = o with { EnableKernelCaching = true });
        return builder;
    }
}
