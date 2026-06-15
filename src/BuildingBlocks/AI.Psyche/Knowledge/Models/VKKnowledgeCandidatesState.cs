using System.Collections.Generic;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// State payload holding knowledge entry candidates for filtering and late binding.
/// </summary>
public sealed class VKKnowledgeCandidatesState
{
    /// <summary>
    /// Gets the list of knowledge candidates.
    /// </summary>
    public List<VKKnowledgeEntry> Candidates { get; } = [];
}
