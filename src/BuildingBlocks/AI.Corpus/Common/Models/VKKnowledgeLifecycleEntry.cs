using VK.Blocks.AI.Psyche;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Represents a corpus entry wrapping a knowledge entry with corpus-specific metadata.
/// </summary>
public sealed record VKKnowledgeLifecycleEntry
{
    /// <summary>
    /// Gets the inner knowledge entry.
    /// </summary>
    public required VKKnowledgeEntry Knowledge { get; init; }

    /// <summary>
    /// Gets the corpus-specific override/evaluation settings for this entry.
    /// </summary>
    public required VKKnowledgeLifecycle Lifecycle { get; init; }
}
