using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VK.Blocks.AI.MEAI.Common.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI.Governance.Internal;

/// <summary>
/// Intercepts chat messages and detects prompt injection attacks before invocation.
/// </summary>
internal sealed class PromptInjectionChatClient(
    IChatClient innerClient,
    IVKInjectionDetector detector,
    ILogger? logger = null)
    : DelegatingChatClient(innerClient)
{
    private readonly IVKInjectionDetector _detector = VKGuard.NotNull(detector);
    private readonly ILogger _logger = logger ?? NullLogger.Instance;

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await VerifyNoInjectionAsync(messages, cancellationToken).ConfigureAwait(false);
        return await base.GetResponseAsync(messages, options, cancellationToken).ConfigureAwait(false);
    }

    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await VerifyNoInjectionAsync(messages, cancellationToken).ConfigureAwait(false);
        await foreach (var update in base.GetStreamingResponseAsync(messages, options, cancellationToken).ConfigureAwait(false))
        {
            yield return update;
        }
    }

    private async Task VerifyNoInjectionAsync(IEnumerable<ChatMessage> messages, CancellationToken ct)
    {
        foreach (var msg in messages)
        {
            if (msg.Role != ChatRole.User)
            {
                continue;
            }

            // Check direct Text property
            if (!string.IsNullOrWhiteSpace(msg.Text))
            {
                await CheckTextAsync(msg.Text, ct).ConfigureAwait(false);
            }

            // Also check all TextContent elements in Contents
            if (msg.Contents is not null)
            {
                foreach (var content in msg.Contents)
                {
                    if (content is TextContent tc && !string.IsNullOrWhiteSpace(tc.Text) && tc.Text != msg.Text)
                    {
                        await CheckTextAsync(tc.Text, ct).ConfigureAwait(false);
                    }
                }
            }
        }
    }

    private async Task CheckTextAsync(string text, CancellationToken ct)
    {
        var detectRes = await _detector.DetectAsync(text, ct).ConfigureAwait(false);
        if (detectRes.IsSuccess && detectRes.Value?.IsInjectionDetected == true)
        {
            var patternType = detectRes.Value.DetectedPatternType ?? "PromptInjection";
            var confidence = detectRes.Value.ConfidenceScore;
            _logger.PromptInjectionDetected(patternType, confidence);

            throw new AISecurityException(VKAIErrors.PromptInjectionDetected, confidence);
        }
    }
}

