using System.Collections.Generic;

namespace VK.Blocks.AI.Corpus.Common.Models.Internal;

/// <summary>
/// State payload passed through the Psyche pipeline context to tracking stages.
/// Follows AP.01 / AP.03.
/// </summary>
internal sealed record CorpusInjectionState(
    IReadOnlyList<VKKnowledgeLifecycleEntry> InjectedEntries,
    int CurrentTurn);
