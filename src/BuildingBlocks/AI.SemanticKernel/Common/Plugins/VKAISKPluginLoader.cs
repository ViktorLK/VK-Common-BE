using System.Collections.Generic;
using Microsoft.SemanticKernel;

namespace VK.Blocks.AI.SemanticKernel.Common.Plugins;

/// <summary>
/// Utility for loading Semantic Kernel plugins.
/// </summary>
public sealed class VKAISKPluginLoader
{
    /// <summary>
    /// Loads plugins from the specified kernel.
    /// </summary>
    public static IEnumerable<KernelPlugin> LoadFromKernel(Microsoft.SemanticKernel.Kernel kernel)
    {
        return kernel.Plugins;
    }
}
