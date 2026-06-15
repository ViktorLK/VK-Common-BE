using System.Collections.Generic;

namespace VK.Blocks.AI.Corpus.Common.Models.Internal;

/// <summary>
/// State payload holding recalled entries from the Gathering stage to pass to the Filtering stage.
/// </summary>
internal sealed record RecalledKnowledgeLifecycleState(IReadOnlyList<VKKnowledgeLifecycleEntry> RecalledEntries);
