using System;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("VK.Blocks.Generators.UnitTests")]

namespace VK.Blocks.Generators.Utilities;

/// <summary>
/// Provides a unified mechanism to guard Source Generator execution based on namespace and assembly naming conventions.
/// </summary>
internal static class VKBlockGeneratorGuard
{
    private const string VKBlockNamespacePrefix = VKBlocksConstants.VKBlocksPrefix;
    private const string VKBlockGeneratorsPrefix = VKBlocksConstants.VKBlocksPrefix + "Generators.";

    private static readonly string[] _globalModules = ["Observability", "Diagnostics", "DependencyInjection"];

    /// <summary>
    /// Determines if a Source Generator should execute for the given assembly based on the generator's namespace.
    /// </summary>
    /// <param name="generatorType">The type of the Source Generator (usually <c>this.GetType()</c>).</param>
    /// <param name="assemblyName">The name of the assembly being compiled.</param>
    /// <returns><c>true</c> if the generator should execute; otherwise, <c>false</c>.</returns>
    public static bool ShouldExecute(Type generatorType, string? assemblyName)
    {
        if (string.IsNullOrEmpty(assemblyName) || !assemblyName!.StartsWith(VKBlockNamespacePrefix, StringComparison.Ordinal))
        {
            return false;
        }

        var ns = generatorType.Namespace;
        if (string.IsNullOrEmpty(ns) || !ns!.StartsWith(VKBlockGeneratorsPrefix, StringComparison.Ordinal))
        {
            return false; // Generators must follow the strict naming convention
        }

        // Extract the module path after "VK.Blocks.Generators."
        var modulePath = ns.Substring(VKBlockGeneratorsPrefix.Length);

        // 1. Global Generators (Observability, Diagnostics) should run on all VK.Blocks projects
        if (_globalModules.Any(m => modulePath.StartsWith(m, StringComparison.Ordinal)))
        {
            return true;
        }

        var assemblySuffix = assemblyName.Substring(VKBlockNamespacePrefix.Length);

        // 2. Control Test Projects Execution
        if (assemblySuffix.Contains("Test", StringComparison.OrdinalIgnoreCase))
        {
            // Allow Generators unit tests to trigger any generator
            if (assemblySuffix.StartsWith("Generators.", StringComparison.Ordinal))
            {
                return true;
            }

            // For other test projects, only trigger generators belonging to the same module
            return assemblySuffix.Equals(modulePath, StringComparison.Ordinal) ||
                   assemblySuffix.StartsWith($"{modulePath}.", StringComparison.Ordinal);
        }

        // 3. Module Specific Generators
        return assemblySuffix.Equals(modulePath, StringComparison.Ordinal) ||
               assemblySuffix.StartsWith($"{modulePath}.", StringComparison.Ordinal);
    }
}
