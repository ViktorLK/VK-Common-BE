using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

using VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;

using VK.Blocks.AI.SemanticKernel.Common.DependencyInjection;

namespace VK.Blocks.AI.SemanticKernel.Common.Plugins.Internal;

/// <summary>
/// A plugin provider that loads plugins based on <see cref="VKAISKDefaultsOptions"/>.
/// </summary>
internal sealed class AISKConfigPluginProvider(IOptions<VKAISKDefaultsOptions> options) : IAISKPluginProvider
{
    private readonly VKAISKDefaultsOptions _options = options.Value;

    public void Register(IKernelBuilder builder, IServiceProvider serviceProvider)
    {
        var pluginOptions = _options.Plugins;

        // 1. Register Explicit Types from Configuration
        foreach (var (name, typeName) in pluginOptions.Types)
        {
            var type = Type.GetType(typeName);
            if (type != null)
            {
                builder.Plugins.Add(KernelPluginFactory.CreateFromType(type, name, serviceProvider));
            }
        }

        // 2. Auto-Discovery
        if (pluginOptions.AutoDiscoveryEnabled)
        {
            var assemblies = ResolveAssemblies(pluginOptions.AssembliesToScan);
            var discoveredPlugins = AISKPluginScanner.Scan(assemblies, serviceProvider);
            foreach (var plugin in discoveredPlugins)
            {
                builder.Plugins.Add(plugin);
            }
        }
    }

    private static IEnumerable<Assembly> ResolveAssemblies(List<string> assemblyNames)
    {
        if (assemblyNames == null || assemblyNames.Count == 0)
        {
            // Default: Scan entry assembly and its referenced assemblies (best effort)
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly == null)
                return Array.Empty<Assembly>();

            var assemblies = new HashSet<Assembly> { entryAssembly };
            foreach (var name in entryAssembly.GetReferencedAssemblies())
            {
                try
                {
                    assemblies.Add(Assembly.Load(name));
                }
                catch { /* Ignore load failures for scan */ }
            }
            return assemblies;
        }

        return assemblyNames
            .Select(name =>
            {
                try
                { return Assembly.Load(name); }
                catch { return null; }
            })
            .Where(a => a != null)!;
    }
}


