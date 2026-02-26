using Microsoft.CodeAnalysis;

namespace VK.Blocks.Generators.Extensions;

internal static class IncrementalValuesProviderExtensions
{
    public static IncrementalValuesProvider<T> WhereNotNull<T>(
        this IncrementalValuesProvider<T?> source) where T : class
        => source.Where(x => x is not null).Select((x, _) => x!);
}
