using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace VK.Blocks.Core.DependencyInjection.Internal;

/// <summary>
/// An internal registry that holds pre-bound/transformed options instances by key/name
/// during the DI configuration phase. Resolves the IOptions dual-binding validation
/// pattern for both default and keyed configuration paths.
/// </summary>
internal sealed class BlockOptionsRegistry<TOptions>
    where TOptions : class, IVKBlockOptions, new()
{
    private readonly Dictionary<string, TOptions> _instances = new(StringComparer.Ordinal);

    public void Set(string name, TOptions options)
    {
        _instances[name] = options;
    }

    public bool TryGet(string name, [NotNullWhen(true)] out TOptions? options)
    {
        return _instances.TryGetValue(name, out options);
    }
}
