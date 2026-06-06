using Microsoft.SemanticKernel;

namespace VK.Blocks.AI.SemanticKernel.Common.Plugins;

/// <summary>
/// Factory for creating Semantic Kernel plugins.
/// </summary>
public sealed class VKPluginFactory
{
    /// <summary>
    /// Creates a plugin from an object instance.
    /// </summary>
    public static KernelPlugin CreateFromObject(object target, string? pluginName = null)
    {
        return KernelPluginFactory.CreateFromObject(target, pluginName);
    }

    /// <summary>
    /// Creates a plugin from a type.
    /// </summary>
    public static KernelPlugin CreateFromType<T>(string? pluginName = null)
    {
        return KernelPluginFactory.CreateFromType<T>(pluginName);
    }
}
