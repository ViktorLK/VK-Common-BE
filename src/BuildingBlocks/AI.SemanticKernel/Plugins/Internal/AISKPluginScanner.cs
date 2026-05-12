using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.SemanticKernel;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Plugins.Internal;

/// <summary>
/// Internal utility for scanning assemblies for VKAI plugins.
/// </summary>
internal static class AISKPluginScanner
{
    public static IEnumerable<KernelPlugin> Scan(IEnumerable<Assembly> assemblies, IServiceProvider serviceProvider)
    {
        VKGuard.NotNull(assemblies);

        foreach (var assembly in assemblies)
        {
            var pluginTypes = assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<VKAIPluginAttribute>() is { AutoRegister: true });

            foreach (var type in pluginTypes)
            {
                var attr = type.GetCustomAttribute<VKAIPluginAttribute>()!;
                var pluginName = attr.Name ?? type.Name;
                yield return KernelPluginFactory.CreateFromType(type, pluginName, serviceProvider);
            }
        }
    }
}
