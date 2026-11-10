namespace VK.Blocks.VectorStore;

/// <summary>
/// Defines a registered logical collection mapping to physical storage and embedding constraints.
/// </summary>
public sealed record VKLogicalCollectionDefinition
{
    /// <summary>
    /// Gets the logical collection name used across application modules.
    /// </summary>
    public required string LogicalName { get; init; }

    /// <summary>
    /// Gets the physical collection name or active alias in the underlying vector database.
    /// Defaults to the logical name if not explicitly overridden.
    /// </summary>
    public string? PhysicalNameOrAlias { get; init; }

    /// <summary>
    /// Gets the expected embedding model identifier (e.g. "text-embedding-3-small").
    /// </summary>
    public string? ExpectedEmbeddingModelId { get; init; }

    /// <summary>
    /// Gets the expected vector dimensions (e.g. 1536).
    /// </summary>
    public int? ExpectedDimensions { get; init; }

    /// <summary>
    /// Resolves the actual physical collection or alias name.
    /// </summary>
    public string ResolvePhysicalName() => !string.IsNullOrWhiteSpace(PhysicalNameOrAlias) ? PhysicalNameOrAlias : LogicalName;
}

