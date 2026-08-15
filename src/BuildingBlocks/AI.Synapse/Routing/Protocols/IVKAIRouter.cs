using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Defines the router contract for prioritizing and sorting available AI connections.
/// </summary>
public interface IVKAIRouter
{
    /// <summary>
    /// Evaluates and orders all eligible candidates based on availability, latency, and context preferences.
    /// </summary>
    Task<VKResult<IReadOnlyList<VKAIConnection>>> ResolveCandidatesAsync(
        VKAIRouteArgs? args,
        IEnumerable<VKAIConnection> pool,
        CancellationToken cancellationToken = default);
}
