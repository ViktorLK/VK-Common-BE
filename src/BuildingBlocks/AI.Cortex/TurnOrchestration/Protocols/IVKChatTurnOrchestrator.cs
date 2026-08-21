using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cortex;

/// <summary>
/// Top-level coarse-grained orchestrator for executing conversational dialogue turns.
/// Receives fully-resolved value objects (<see cref="VKChatTurnRequest"/>) from the App layer and encapsulates
/// pipeline weaving (AI.Psyche), structured negotiation &amp; binding (AI.Eidos), resilience strategies, and idempotent echo persistence.
/// Follows CS.01, CS.03.
/// </summary>
public interface IVKChatTurnOrchestrator
{
    /// <summary>
    /// Processes a single dialogue turn returning a plain text result.
    /// </summary>
    Task<VKResult<VKChatTurnResult>> ProcessTurnAsync(
        VKChatTurnRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a single dialogue turn returning a strongly-typed structured DTO result via AI.Eidos.
    /// </summary>
    /// <typeparam name="TDto">The expected DTO response type.</typeparam>
    Task<VKResult<VKChatTurnResult<TDto>>> ProcessTurnAsync<TDto>(
        VKChatTurnRequest request,
        CancellationToken cancellationToken = default) where TDto : class;

    /// <summary>
    /// Previews the woven prompt tapestry without executing the model call (WeaveOnly mode).
    /// </summary>
    Task<VKResult<VKPsycheResponse>> PreviewPromptAsync(
        VKChatTurnRequest request,
        CancellationToken cancellationToken = default);
}
