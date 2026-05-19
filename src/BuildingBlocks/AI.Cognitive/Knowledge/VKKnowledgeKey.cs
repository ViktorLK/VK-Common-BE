namespace VK.Blocks.AI.Cognitive;

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
    /// Gets a value indicating whether the trigger pattern is a regular expression.
    /// </summary>
    public bool IsRegex { get; init; }

    /// <summary>
    /// Gets a value indicating whether this key acts as a post-filter rather than a primary trigger.
    /// </summary>
    public bool IsFilter { get; init; }

    /// <summary>
    /// Gets the logical combination rules used when matching multiple keys.
    /// </summary>
    public VKKnowledgeFilterLogic Logic { get; init; } = VKKnowledgeFilterLogic.AndAny;

    /// <summary>
    /// Gets a value indicating whether this key must match as a whole word (using word boundaries \b).
    /// </summary>
    public bool MatchWholeWord { get; init; } = false;

    /// <summary>
    /// Gets a value indicating whether the match is case-sensitive.
    /// </summary>
    public bool CaseSensitive { get; init; } = false;
}
