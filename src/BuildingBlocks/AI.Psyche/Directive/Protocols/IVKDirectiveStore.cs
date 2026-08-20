using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain contract to resolve Directives.
/// Follows CS.01, CS.03, and CS.04 batching patterns.
/// </summary>
public interface IVKDirectiveStore
{
    /// <summary>
    /// Resolves the Directives containing prompts and safety rules for the specified directive IDs.
    /// </summary>
    Task<VKResult<IReadOnlyList<VKDirectiveCharter>>> GetDirectivesAsync(
        IReadOnlyList<VKDirectiveId> directiveIds,
        CancellationToken cancellationToken = default);
}
