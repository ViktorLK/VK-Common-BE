using System.Collections.Generic;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Represents an entry in a knowledge/worldbook.
/// </summary>
public sealed record VKKnowledgeEntry
{
    /// <summary>
    /// Gets the unique identifier for the entry.
    /// </summary>
    public required string Id { get; init; }

    public required string KnowledgeBookId { get; init; }

    /// <summary>
    /// Gets a value indicating whether this entry is active and enabled for retrieval.
    /// </summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>
    /// Gets the structured keys that trigger this entry.
    /// </summary>
    public IReadOnlyList<VKKnowledgeKey> Keys { get; init; } = [];

    /// <summary>
    /// Gets the content of the entry.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Gets the trigger type.
    /// </summary>
    public VKKnowledgeTriggerType TriggerType { get; init; } = VKKnowledgeTriggerType.Keyword;

    /// <summary>
    /// Gets the optional developer memo or description about this entry.
    /// </summary>
    public string? Memo { get; init; }

    /// <summary>
    /// Gets the prompt-weaving and recursion limit configuration.
    /// Defaults to <see cref="VKKnowledgeWeavingRules.Default"/>.
    /// <remarks>
    /// DB Mapping: Configured as an EF Core Owned Type via <c>OwnsOne(e => e.Weaving)</c>
    /// to flatten fields into the same table or store as a JSON column (Value Object design).
    /// </remarks>
    /// </summary>
    public VKKnowledgeWeavingRules Weaving { get; init; } = VKKnowledgeWeavingRules.Default;
}
