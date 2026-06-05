using System;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("VK.Tools.SourceGenerators.UnitTests")]

namespace VK.Tools.SourceGenerators.Utilities;

/// <summary>
/// Provides a unified mechanism to guard Source Generator execution based on namespace and assembly naming conventions.
/// </summary>
internal static class VKBlockGeneratorGuard
{
    private const string VKBlockNamespacePrefix = $"{VKBlocksConstants.VKBlocksPrefix}.";
    private const string VKLabNamespacePrefix = "VK.Labs.";
    private const string VKToolNamespacePrefix = "VK.Tools.";
    private const string VKBlockGeneratorsPrefix = "VK.Tools.SourceGenerators.";

    private static readonly string[] _globalModules = ["Observability", "Diagnostics", "DependencyInjection", "Domain"];

    /// <summary>
    /// Determines if a Source Generator should execute for the given assembly based on the generator's namespace.
    /// </summary>
    /// <param name="generatorType">The type of the Source Generator (usually <c>this.GetType()</c>).</param>
    /// <param name="assemblyName">The name of the assembly being compiled.</param>
    /// <returns><c>true</c> if the generator should execute; otherwise, <c>false</c>.</returns>
    public static bool ShouldExecute(Type generatorType, string? assemblyName)
    {
        if (string.IsNullOrEmpty(assemblyName))
        {
            return false;
        }

        bool isVkBlock = assemblyName!.StartsWith(VKBlockNamespacePrefix, StringComparison.Ordinal);
        bool isVkLab = assemblyName!.StartsWith(VKLabNamespacePrefix, StringComparison.Ordinal);
        bool isVkTool = assemblyName!.StartsWith(VKToolNamespacePrefix, StringComparison.Ordinal);

        if (!isVkBlock && !isVkLab && !isVkTool)
        {
            return false;
        }

        var ns = generatorType.Namespace;
        if (string.IsNullOrEmpty(ns) || !ns!.StartsWith(VKBlockGeneratorsPrefix, StringComparison.Ordinal))
        {
            return false; // Generators must follow the strict naming convention
        }

        // Extract the module path after "VK.Tools.SourceGenerators."
        var modulePath = ns.Substring(VKBlockGeneratorsPrefix.Length);

        // 1. Global Generators (Observability, Diagnostics) should run on all VK projects
        if (_globalModules.Any(m => modulePath.StartsWith(m, StringComparison.Ordinal)))
        {
            return true;
        }

        string prefix = isVkBlock ? VKBlockNamespacePrefix : (isVkLab ? VKLabNamespacePrefix : VKToolNamespacePrefix);
        var assemblySuffix = assemblyName.Substring(prefix.Length);

        // 2. Control Test Projects Execution
        if (assemblySuffix.Contains("Test", StringComparison.OrdinalIgnoreCase))
        {
            // Allow SourceGenerators unit tests to trigger any generator
            if (assemblySuffix.StartsWith("SourceGenerators.", StringComparison.Ordinal))
            {
                return true;
            }

            // For other test projects, only trigger generators belonging to the same module
            return assemblySuffix.Equals(modulePath, StringComparison.Ordinal) ||
                   assemblySuffix.StartsWith($"{modulePath}.", StringComparison.Ordinal);
        }

        // For Labs and Tools (except SG tests), currently we only support global generators unless we define module mapping
        if (isVkLab || isVkTool)
        {
            return false;
        }

        // 3. Module Specific Generators
        return assemblySuffix.Equals(modulePath, StringComparison.Ordinal) ||
               assemblySuffix.StartsWith($"{modulePath}.", StringComparison.Ordinal);
    }
}
