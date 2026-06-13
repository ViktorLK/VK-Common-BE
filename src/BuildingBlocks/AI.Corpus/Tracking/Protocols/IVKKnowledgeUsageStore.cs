using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Defines the storage contract for reading and writing knowledge/corpus usage logs.
/// Follows CS.01 / CS.03.
/// </summary>
public interface IVKKnowledgeUsageStore
{
    /// <summary>
    /// Records that specific corpus entries were injected during a turn.
    /// </summary>
    Task<VKResult> RecordUsageAsync(
        string sessionId,
        int turn,
        string entryId,
        string tag,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds the execution context containing session usage history up to the current turn.
    /// </summary>
    Task<VKResult<VKCorpusContext>> GetContextAsync(
        string sessionId,
        int currentTurn,
        CancellationToken cancellationToken = default);
}
