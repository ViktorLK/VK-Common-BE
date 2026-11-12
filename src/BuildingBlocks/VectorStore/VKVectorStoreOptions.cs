using System.Collections.Concurrent;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.VectorStore;

/// <summary>
/// Default configuration settings for the AI Vector Store building block.
/// Holds the logical collection registry, alias routes, and default query bounds.
/// </summary>
[VKOptions]
public sealed partial record VKVectorStoreOptions : IVKBlockOptions
{
    private readonly ConcurrentDictionary<string, string> _runtimeAliasOverrides = new();

    /// <summary>
    /// Gets the type of the vector store.
    /// </summary>
    public VKVectorStoreType Type { get; init; } = VKVectorStoreType.InMemory;

    /// <summary>
    /// Gets the default collection name.
    /// </summary>
    public string DefaultCollection { get; init; } = "default";

    /// <summary>
    /// Gets the default maximum number of results to return.
    /// </summary>
    public int DefaultLimit { get; init; } = 5;

    /// <summary>
    /// Gets the default minimum similarity score (0.0 to 1.0).
    /// </summary>
    public float DefaultMinScore { get; init; } = 0.7f;

    /// <summary>
    /// Gets the dictionary of declared logical collections for alias routing and dimension validation.
    /// </summary>
    public Dictionary<string, VKLogicalCollectionDefinition> LogicalCollections { get; init; } = [];

    /// <summary>
    /// Resolves the physical collection or alias name for a given logical collection name.
    /// Prioritizes dynamic runtime alias overrides for blue-green re-embedding.
    /// </summary>
    public string ResolvePhysicalCollectionName(string logicalName)
    {
        VKGuard.NotNullOrWhiteSpace(logicalName);

        if (_runtimeAliasOverrides.TryGetValue(logicalName, out var overriddenPhysical) && !string.IsNullOrWhiteSpace(overriddenPhysical))
        {
            return overriddenPhysical;
        }

        if (LogicalCollections.TryGetValue(logicalName, out var definition) && definition is not null)
        {
            return definition.ResolvePhysicalName();
        }

        return logicalName;
    }

    /// <summary>
    /// Dynamically switches the active physical collection or alias routed to by a logical collection.
    /// Supports zero-downtime blue-green index rotation.
    /// </summary>
    public void SwitchActiveAlias(string logicalName, string newPhysicalNameOrAlias)
    {
        VKGuard.NotNullOrWhiteSpace(logicalName);
        VKGuard.NotNullOrWhiteSpace(newPhysicalNameOrAlias);

        _runtimeAliasOverrides[logicalName] = newPhysicalNameOrAlias;
    }
}
