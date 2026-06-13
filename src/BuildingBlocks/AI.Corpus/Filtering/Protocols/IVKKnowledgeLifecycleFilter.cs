using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Defines a filter that determines if a corpus entry should be injected.
/// </summary>
public interface IVKKnowledgeLifecycleFilter
{
    /// <summary>
    /// Evaluates if the entry matches the filter criteria to be included.
    /// </summary>
    Task<VKResult<VKFilterVerdict>> EvaluateAsync(
        VKKnowledgeLifecycleEntry entry,
        VKCorpusContext context,
        CancellationToken cancellationToken = default);
}
