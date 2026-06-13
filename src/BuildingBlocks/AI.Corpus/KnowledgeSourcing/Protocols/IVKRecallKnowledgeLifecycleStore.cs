using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Defines a store for dynamically recalling knowledge lifecycle entries.
/// </summary>
public interface IVKRecallKnowledgeLifecycleStore
{
    /// <summary>
    /// Retrieves a list of candidate knowledge lifecycle entries.
    /// </summary>
    Task<VKResult<IReadOnlyList<VKKnowledgeLifecycleEntry>>> GetLifecycleEntriesAsync(
        VKCorpusContext context,
        CancellationToken cancellationToken = default);
}
