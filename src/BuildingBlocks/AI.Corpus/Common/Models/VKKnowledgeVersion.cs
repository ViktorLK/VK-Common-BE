using System;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Immutable snapshot model representing a specific historical version of a corpus knowledge entry.
/// </summary>
public sealed record VKKnowledgeVersion
{
    /// <summary>
    /// Gets the unique document identifier.
    /// </summary>
    public required string DocumentId { get; init; }

    /// <summary>
    /// Gets the target collection name for isolation.
    /// </summary>
    public required string CollectionName { get; init; }

    /// <summary>
    /// Gets the version number (e.g. 1, 2, 3).
    /// </summary>
    public required int Version { get; init; }

    /// <summary>
    /// Gets the raw text content of the version snapshot.
    /// </summary>
    public required string RawText { get; init; }

    /// <summary>
    /// Gets the knowledge lifecycle settings associated with this version.
    /// </summary>
    public required VKKnowledgeLifecycle Lifecycle { get; init; }

    /// <summary>
    /// Gets the UTC timestamp when this version was created.
    /// </summary>
    public required DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>
    /// Gets the optional user or system author who created this version.
    /// </summary>
    public string? CreatedBy { get; init; }
}
