using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.AI.Cognitive.Engram.Internal;

/// <summary>
/// Defines a synapse that captures AI usage and cost metadata after a request is completed.
/// Useful for billing, quota management, and performance monitoring.
/// </summary>
internal interface IVKAIUsageSynapse // [AP.03]
{
    /// <summary>
    /// Gets the priority of the synapse (lower values fire first).
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Records the usage data for a completed AI request.
    /// </summary>
    /// <param name="context">The context of the request (contains TraceId, TenantId, etc.).</param>
    /// <param name="usage">The token usage and cost data.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask RecordAsync(VKAIRequestContext context, VKTokenUsage usage, CancellationToken ct = default); // [CS.03]
}
