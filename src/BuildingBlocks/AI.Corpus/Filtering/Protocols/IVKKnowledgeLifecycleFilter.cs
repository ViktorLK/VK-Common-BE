using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Defines a filter that determines if a corpus entry should be injected.
/// </summary>
public interface IVKKnowledgeLifecycleFilter : IVKEntryFilter<VKKnowledgeLifecycleEntry, VKCorpusContext>;
