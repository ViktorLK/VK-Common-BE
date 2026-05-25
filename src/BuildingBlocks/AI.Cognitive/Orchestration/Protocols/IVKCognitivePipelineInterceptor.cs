using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Interceptor contract to modify the execution of the cognitive orchestration pipeline.
/// Enables modular features (like bio-realistic simulation or auditing) to hook into pipeline stages.
/// Follows AP.01, AP.03, CS.01, and CS.03.
/// </summary>
public interface IVKCognitivePipelineInterceptor
{
    /// <summary>
    /// Gets the priority of the interceptor (lower values run first).
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Executed before the Chat Engine completes its inference.
    /// Can inject customized System Instructions, modify history, and alter context args.
    /// </summary>
    /// <param name="context">The active pipeline execution context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating operation status.</returns>
    Task<VKResult> OnBeforeChatAsync(
        VKCognitivePipelineContext context,
        CancellationToken cancellationToken = default); // [CS.03]

    /// <summary>
    /// Executed after the Chat Engine completes its inference successfully.
    /// Runs out-of-band to estimate sentiment, log analytics, adjust metabolic states, or evolve persona traits.
    /// </summary>
    /// <param name="context">The active pipeline execution context.</param>
    /// <param name="chatResponse">The raw output from the Chat Engine.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating operation status.</returns>
    Task<VKResult> OnAfterChatAsync(
        VKCognitivePipelineContext context,
        VKChatMessage chatResponse,
        CancellationToken cancellationToken = default); // [CS.03]
}
