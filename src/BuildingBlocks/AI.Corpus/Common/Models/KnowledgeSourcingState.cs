using System.Collections.Generic;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// State payload holding recalled entries from the Retrieval stage to pass to the Filtering stage.
/// </summary>
internal sealed record KnowledgeSourcingState(IReadOnlyList<VKKnowledgeLifecycleEntry> RecalledEntries);
