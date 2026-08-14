using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Service contract for dispatching and executing AI requests with multi-provider routing and fallback.
/// </summary>
public interface IVKAIRouteDispatcher
{
    /// <summary>
    /// Selects the best available candidate provider connection for the given route args.
    /// </summary>
    Task<VKResult<VKAIConnection>> SelectCandidateAsync(
        VKAIRouteArgs? args = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a text/chat operation with automatic resilience, circuit breaking, and cross-provider fallback.
    /// </summary>
    Task<VKResult<TResponse>> ExecuteWithFallbackAsync<TResponse>(
        VKAIRouteArgs? args,
        Func<VKAIConnection, CancellationToken, Task<VKResult<TResponse>>> operation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a chat completion operation through resolved Keyed <see cref="IVKChatEngine"/> instances with automatic fallback.
    /// </summary>
    Task<VKResult<VKChatResponse>> ExecuteChatWithFallbackAsync(
        IEnumerable<VKChatMessage> messages,
        VKAIRouteArgs? args = null,
        CancellationToken cancellationToken = default);
}
