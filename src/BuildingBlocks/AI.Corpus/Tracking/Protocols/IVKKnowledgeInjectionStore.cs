using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Defines the storage contract for reading and writing knowledge/corpus injection logs.
/// Follows CS.01 / CS.03.
/// </summary>
public interface IVKKnowledgeInjectionStore
{
    /// <summary>
    /// Records that specific corpus entries were injected.
    /// </summary>
    Task<VKResult> RecordInjectionsAsync(
        VKSessionId sessionId,
        IReadOnlyCollection<VKKnowledgeInjection> injections,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the session injection history.
    /// </summary>
    Task<VKResult<IReadOnlyCollection<VKKnowledgeInjection>>> GetInjectionsAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default);
}
