using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.AI.Afferent.Common.DependencyInjection.Internal;

namespace VK.Blocks.AI.Afferent;

/// <summary>
/// Service collection extension methods for registering the AI Afferent block.
/// </summary>
public static class VKAIAfferentBlockExtensions
{
    /// <summary>
    /// Adds the AI.Afferent building block to the service collection.
    /// </summary>
    public static IVKAIAfferentBuilder AddVKAIAfferentBlock(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<VKAIAfferentOptions, VKAIAfferentOptions>? configure = null)
        => AIAfferentBlockRegistration.Register(services, configuration, configure);
}
