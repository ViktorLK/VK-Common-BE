using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.MEAI.Common.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI.Chat.Internal;

/// <summary>
/// Intercepts chat messages and logs request execution, duration, and token usage via ILogger.
/// Adheres to CS.06 (TimeProvider injected) and OR.01 ([LoggerMessage]).
/// </summary>
internal sealed class AuditChatClient : DelegatingChatClient
{
    private readonly ILogger _logger;
    private readonly TimeProvider _timeProvider;

    public AuditChatClient(IChatClient innerClient, ILogger logger, TimeProvider? timeProvider = null)
        : base(innerClient)
    {
        _logger = VKGuard.NotNull(logger);
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var startTimestamp = _timeProvider.GetTimestamp();
        try
        {
            var response = await base.GetResponseAsync(messages, options, cancellationToken).ConfigureAwait(false);
            var durationMs = _timeProvider.GetElapsedTime(startTimestamp).TotalMilliseconds;
            var promptTokens = response.Usage?.InputTokenCount ?? 0;
            var completionTokens = response.Usage?.OutputTokenCount ?? 0;
            _logger.ChatCompleted(durationMs, response.ModelId, promptTokens, completionTokens);
            return response;
        }
        catch (Exception ex)
        {
            var durationMs = _timeProvider.GetElapsedTime(startTimestamp).TotalMilliseconds;
            _logger.ChatFailed(ex, durationMs);
            throw;
        }
    }

    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var startTimestamp = _timeProvider.GetTimestamp();
        IAsyncEnumerator<ChatResponseUpdate> enumerator;
        try
        {
            enumerator = base.GetStreamingResponseAsync(messages, options, cancellationToken).GetAsyncEnumerator(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.StreamingInitFailed(ex);
            throw;
        }

        try
        {
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                yield return enumerator.Current;
            }
            var durationMs = _timeProvider.GetElapsedTime(startTimestamp).TotalMilliseconds;
            _logger.StreamingCompleted(durationMs);
        }
        finally
        {
            await enumerator.DisposeAsync().ConfigureAwait(false);
        }
    }
}
