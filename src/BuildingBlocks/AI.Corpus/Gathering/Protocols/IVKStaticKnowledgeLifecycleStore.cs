using System.Collections.Generic;
using VK.Blocks.AI.Psyche;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Defines a store for querying statically configured knowledge lifecycle rules.
/// </summary>
public interface IVKStaticKnowledgeLifecycleStore
{
    /// <summary>
    /// Gets a batch of static knowledge lifecycle entries by their unique identifiers.
    /// </summary>
    IReadOnlyDictionary<VKKnowledgeId, VKKnowledgeLifecycleEntry> GetLifecycleEntries(IEnumerable<VKKnowledgeId> ids);
}
