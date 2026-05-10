using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using VK.Blocks.AI.SemanticKernel.Kernel.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel;

/// <summary>
/// Extension methods for registering Semantic Kernel plugins.
/// </summary>
public static class VKAISKPluginExtensions
{
    /// <summary>
    /// Adds a plugin from a type to the Semantic Kernel.
    /// </summary>
    /// <typeparam name="T">The type of the plugin.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="pluginName">The optional name of the plugin.</param>
    /// <returns>The builder.</returns>
    public static IVKAISKBuilder AddPlugin<T>(this IVKAISKBuilder builder, string? pluginName = null) where T : class
    {
        VKGuard.NotNull(builder);
        builder.Services.AddSingleton<IAISKPluginProvider>(new AISKDelegatePluginProvider((k, sp) => k.Plugins.Add(KernelPluginFactory.CreateFromType<T>(pluginName, sp))));
        return builder;
    }

    /// <summary>
    /// Adds a plugin instance to the Semantic Kernel.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="instance">The plugin instance.</param>
    /// <param name="pluginName">The optional name of the plugin.</param>
    /// <returns>The builder.</returns>
    public static IVKAISKBuilder AddPlugin(this IVKAISKBuilder builder, object instance, string? pluginName = null)
    {
        VKGuard.NotNull(builder);
        VKGuard.NotNull(instance);
        builder.Services.AddSingleton<IAISKPluginProvider>(new AISKDelegatePluginProvider((k, _) => k.Plugins.Add(KernelPluginFactory.CreateFromObject(instance, pluginName))));
        return builder;
    }

    /// <summary>
    /// Adds a plugin by its name and functions.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="pluginName">The name of the plugin.</param>
    /// <param name="functions">The functions in the plugin.</param>
    /// <returns>The builder.</returns>
    public static IVKAISKBuilder AddPlugin(this IVKAISKBuilder builder, string pluginName, System.Collections.Generic.IEnumerable<KernelFunction> functions)
    {
        VKGuard.NotNull(builder);
        VKGuard.NotNullOrWhiteSpace(pluginName);
        VKGuard.NotNull(functions);
        builder.Services.AddSingleton<IAISKPluginProvider>(new AISKDelegatePluginProvider((k, _) => k.Plugins.Add(KernelPluginFactory.CreateFromFunctions(pluginName, functions))));
        return builder;
    }
}
