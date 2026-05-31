namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Represents a structured trigger key for knowledge entry matching.
/// Follows AP.01 (Sealed Record) and AP.03.
/// </summary>
public sealed record VKKnowledgeKey
{
    /// <summary>
    /// Gets the match text or regular expression pattern.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Gets the logical matching strategy used when evaluating this trigger key.
    /// Defaults to <see cref="VKKnowledgeMatchType.Contains"/>.
    /// </summary>
    public VKKnowledgeMatchType MatchType { get; init; } = VKKnowledgeMatchType.Contains;

    /// <summary>
    /// Gets a value indicating whether the match is case-sensitive.
    /// </summary>
    public bool CaseSensitive { get; init; } = false;
}
