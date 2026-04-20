using System.Linq;
using Microsoft.CodeAnalysis;

namespace VK.Blocks.Generators.Extensions;

/// <summary>
/// Provides extension methods for Roslyn symbols to simplify common generator tasks.
/// </summary>
internal static class SymbolExtensions
{
    /// <summary>
    /// Checks if a type symbol implements a specific interface by name.
    /// </summary>
    /// <param name="symbol">The type symbol to check.</param>
    /// <param name="interfaceName">The name of the interface to look for.</param>
    /// <returns>True if the symbol implements the interface; otherwise, false.</returns>
    public static bool ImplementsInterface(this INamedTypeSymbol? symbol, string interfaceName)
    {
        if (symbol is null)
        {
            return false;
        }

        return symbol.AllInterfaces.Any(i =>
            i.Name == interfaceName || i.ToDisplayString().EndsWith(interfaceName));
    }

    /// <summary>
    /// Checks if a symbol has a specific attribute by metadata name.
    /// </summary>
    /// <param name="symbol">The symbol to check.</param>
    /// <param name="attributeFullName">The full metadata name of the attribute.</param>
    /// <returns>True if the symbol has the attribute; otherwise, false.</returns>
    public static bool HasAttribute(this ISymbol? symbol, string attributeFullName)
    {
        if (symbol is null)
        {
            return false;
        }

        return symbol.GetAttributes().Any(a =>
            a.AttributeClass?.ToDisplayString() == attributeFullName);
    }

    /// <summary>
    /// Checks if a type symbol inherits from or is a specific class by name.
    /// </summary>
    /// <param name="symbol">The type symbol to check.</param>
    /// <param name="baseClassName">The name of the base class to look for.</param>
    /// <returns>True if the symbol inherits from or is the specified class; otherwise, false.</returns>
    public static bool InheritsFromOrIs(this INamedTypeSymbol? symbol, string baseClassName)
    {
        var current = symbol;
        while (current is not null)
        {
            if (current.Name == baseClassName || current.ToDisplayString().EndsWith(baseClassName))
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Gets the full metadata name of the symbol.
    /// </summary>
    /// <param name="symbol">The symbol to get the namespace for.</param>
    /// <returns>The full namespace display string, or an empty string if it's the global namespace.</returns>
    public static string GetFullNamespace(this ISymbol symbol)
    {
        if (symbol.ContainingNamespace is null || symbol.ContainingNamespace.IsGlobalNamespace)
        {
            return string.Empty;
        }

        return symbol.ContainingNamespace.ToDisplayString();
    }
}
