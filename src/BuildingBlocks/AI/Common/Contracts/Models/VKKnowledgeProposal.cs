namespace VK.Blocks.AI;

/// <summary>
/// Knowledge proposal evaluated by PFC cognitive reflection.
/// Shared DTO definition located in VK.Blocks.AI contract library for zero-coupling cross-block access.
/// </summary>
public sealed record VKKnowledgeProposal
{
    /// <summary>
    /// Gets the extracted knowledge or fact content text.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Gets the confidence score of the extracted proposal (0.0 to 1.0).
    /// Used by Corpus ingestion to determine pending_review gate (<0.85 requires review).
    /// </summary>
    public double Confidence { get; init; } = 0.8;

    /// <summary>
    /// Gets the category of the knowledge (e.g. Preference, Fact, TopicInterest, EmotionalTrigger).
    /// </summary>
    public string Category { get; init; } = "Fact";

    /// <summary>
    /// Gets the sensitivity level of the knowledge (Public, Private, Secret).
    /// Used to enforce UserSegmentFilter and target user isolation.
    /// </summary>
    public string Sensitivity { get; init; } = "Private";

    /// <summary>
    /// Gets the real-time injection frequency control axis (PersistentActive, TopicContext, PassiveRecall, Flash).
    /// Maps to Corpus StickyTurns and KnowledgeCooldownTurns.
    /// </summary>
    public string InjectionFrequency { get; init; } = "TopicContext";

    /// <summary>
    /// Gets the long-term storage retention policy axis (Flash, Transient, LongTerm, Permanent).
    /// Maps to Engram BaseRetentionScore and decay factor.
    /// </summary>
    public string RetentionPolicy { get; init; } = "LongTerm";
}
