using System.Collections.Generic;

namespace VK.Blocks.AI.VectorStore.VectorStore.Models;

/// <summary>
/// Represents a metadata filter for vector search operations.
/// Serves as a placeholder for advanced metadata filtering (e.g., TenantId == "A" AND Status == "Active").
/// </summary>
public sealed record VKMetadataFilter
{
    // A placeholder for actual filter expressions.
    // In a full implementation, this could use an expression tree or a specialized query language.
    public Dictionary<string, object> EqualityFilters { get; init; } = new();
}
