using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Guardrails.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Guardrails.Content.Internal;

/// <summary>
/// A decorator that wraps an <see cref="IVKChatEngine"/> to enforce content governance and safety moderation.
/// </summary>
internal sealed class VKGovernanceChatDecorator : IVKChatEngine
{
    private readonly IVKChatEngine _inner;
    private readonly IVKModerationEngine _moderator;
    private readonly IOptions<VKContentOptions> _options;
    private readonly ILogger<VKGovernanceChatDecorator> _logger;

    public VKGovernanceChatDecorator(
        IVKChatEngine inner,
        IVKModerationEngine moderator,
        IOptions<VKContentOptions> options,
        ILogger<VKGovernanceChatDecorator> logger)
    {
        _inner = VKGuard.NotNull(inner);
        _moderator = VKGuard.NotNull(moderator);
        _options = VKGuard.NotNull(options);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<VKChatResponse>> SendAsync(
        IEnumerable<VKChatMessage> history,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        if (_options.Value.EnableContentFilter != true)
        {
            return await _inner.SendAsync(history, args, cancellationToken).ConfigureAwait(false);
        }

        // 1. Check Input
        var lastUserMessage = history.LastOrDefault(m => m.Role == VKChatRole.User);
        if (lastUserMessage is not null && !string.IsNullOrWhiteSpace(lastUserMessage.Content))
        {
            var moderationResult = await _moderator.CheckContentAsync(lastUserMessage.Content, cancellationToken).ConfigureAwait(false);
            if (!moderationResult.IsSuccess)
            {
                return VKResult.Failure<VKChatResponse>(moderationResult.Errors);
            }

            if (moderationResult.Value.IsFlagged)
            {
                GuardrailsDiagnostics.ContentFlagged(_logger, moderationResult.Value.FlaggedReason);
                return VKResult.Failure<VKChatResponse>(VKAIErrors.SafetyViolation(moderationResult.Value.FlaggedReason ?? "Inappropriate input content"));
            }
        }

        // 2. Call Inner Engine
        var chatResult = await _inner.SendAsync(history, args, cancellationToken).ConfigureAwait(false);
        if (!chatResult.IsSuccess)
        {
            return chatResult;
        }

        // 3. Check Output
        if (!string.IsNullOrWhiteSpace(chatResult.Value.Message.Content))
        {
            var moderationResult = await _moderator.CheckContentAsync(chatResult.Value.Message.Content, cancellationToken).ConfigureAwait(false);
            if (!moderationResult.IsSuccess)
            {
                return VKResult.Failure<VKChatResponse>(moderationResult.Errors);
            }

            if (moderationResult.Value.IsFlagged)
            {
                GuardrailsDiagnostics.ContentFlagged(_logger, moderationResult.Value.FlaggedReason);
                return VKResult.Failure<VKChatResponse>(VKAIErrors.SafetyViolation(moderationResult.Value.FlaggedReason ?? "Inappropriate output content"));
            }
        }

        return chatResult;
    }

    public async IAsyncEnumerable<VKResult<VKChatStreamingResponse>> SendStreamingAsync(
        IEnumerable<VKChatMessage> history,
        IVKAIArgs? args = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_options.Value.EnableContentFilter != true)
        {
            await foreach (var chunk in _inner.SendStreamingAsync(history, args, cancellationToken).ConfigureAwait(false))
            {
                yield return chunk;
            }
            yield break;
        }

        // 1. Check Input
        var lastUserMessage = history.LastOrDefault(m => m.Role == VKChatRole.User);
        if (lastUserMessage is not null && !string.IsNullOrWhiteSpace(lastUserMessage.Content))
        {
            var moderationResult = await _moderator.CheckContentAsync(lastUserMessage.Content, cancellationToken).ConfigureAwait(false);
            if (!moderationResult.IsSuccess)
            {
                yield return VKResult.Failure<VKChatStreamingResponse>(moderationResult.Errors);
                yield break;
            }

            if (moderationResult.Value.IsFlagged)
            {
                GuardrailsDiagnostics.ContentFlagged(_logger, moderationResult.Value.FlaggedReason);
                yield return VKResult.Failure<VKChatStreamingResponse>(VKAIErrors.SafetyViolation(moderationResult.Value.FlaggedReason ?? "Inappropriate input content"));
                yield break;
            }
        }

        // 2. Stream Inner Engine
        // Note: Real-time streaming moderation is complex. A simple approach is to moderate chunks,
        // but this often fails since bad words can be split across chunks.
        // Advanced implementations use a sliding window buffer.
        // For now, we trust the model output more if input was safe, or we buffer (which breaks streaming).
        // Since this is a basic decorator, we'll stream as-is, but a real app might need delayed chunk emitting.
        await foreach (var chunk in _inner.SendStreamingAsync(history, args, cancellationToken).ConfigureAwait(false))
        {
            yield return chunk;
        }
    }
}
