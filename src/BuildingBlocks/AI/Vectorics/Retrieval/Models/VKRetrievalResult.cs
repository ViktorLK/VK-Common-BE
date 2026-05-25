using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Represents a single piece of retrieved knowledge or context.
/// </summary>
public sealed record VKRetrievalResult
{
    /// <summary>
    /// Gets the text content of the retrieved knowledge.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Gets the relevance score of the retrieved item (0.0 to 1.0).
    /// </summary>
    public required double Score { get; init; }

    /// <summary>
    /// Gets the source URL or document URI, if available.
    /// </summary>
    public string? SourceUrl { get; init; }

    /// <summary>
    /// Gets the page number or section identifier, if available.
    /// </summary>
    public string? PageNumber { get; init; }

    /// <summary>
    /// Gets additional metadata associated with the retrieved item.
    /// </summary>
    public IDictionary<string, object?>? Metadata { get; init; }
}
