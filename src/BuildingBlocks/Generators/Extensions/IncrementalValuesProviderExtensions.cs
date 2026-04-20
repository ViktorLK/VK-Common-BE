using System.Linq;
using Microsoft.CodeAnalysis;

namespace VK.Blocks.Generators.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IncrementalValuesProvider{T}"/>.
/// </summary>
internal static class IncrementalValuesProviderExtensions
{
    /// <summary>
    /// Filters out null values from the source provider.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The source provider.</param>
    /// <returns>A new provider that only contains non-null values.</returns>
    public static IncrementalValuesProvider<T> WhereNotNull<T>(
        this IncrementalValuesProvider<T?> source) where T : class
        => source.Where(x => x is not null).Select((x, _) => x!);
}
