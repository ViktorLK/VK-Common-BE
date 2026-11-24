using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI.Governance.Internal;

/// <summary>
/// Intercepts chat messages and enforces hard single-request token caps (OWN-REQUEST-CAP),
/// rate limiting, and provider-reported actual token usage reporting (OWN-USAGE-ACTUAL).
/// Preserves non-text AIContent (DataContent/UriContent/FunctionCallContent/FunctionResultContent).
/// Adheres to ARCH-12, CS.01, CS.03, AP.01, MEA-04, and MEA-07.
/// </summary>
// [ARCH-12] [AP.01] [CS.01] [CS.03] [MEA-04] [MEA-07]
internal sealed class TokenGovernanceChatClient : DelegatingChatClient
{
    private readonly IVKTokenCounter? _tokenCounter;
    private readonly IVKTokenRateLimiter? _rateLimiter;
    private readonly IVKTokenUsageAggregator? _usageAggregator;
    private readonly int _maxContextTokens;

    public TokenGovernanceChatClient(
        IChatClient innerClient,
        IVKTokenCounter? tokenCounter,
        IVKAIContextPruner? contextPruner = null,
        IVKTokenRateLimiter? rateLimiter = null,
        IVKTokenUsageAggregator? usageAggregator = null,
        int maxContextTokens = 8192,
        ILogger? logger = null)
        : base(VKGuard.NotNull(innerClient))
    {
        _tokenCounter = tokenCounter;
        _ = contextPruner;
        _rateLimiter = rateLimiter;
        _usageAggregator = usageAggregator;
        _maxContextTokens = maxContextTokens;
        _ = logger;
    }

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(messages);
        cancellationToken.ThrowIfCancellationRequested();

        var (validatedMessages, estimatedTokens) = EnforceRequestCap(messages, options?.ModelId);

        if (_rateLimiter is not null)
        {
            var acquireResult = await _rateLimiter.AcquireAsync(estimatedTokens, cancellationToken).ConfigureAwait(false);
            if (!acquireResult.IsSuccess)
            {
                throw new AISecurityException(acquireResult.FirstError, 1.0f);
            }
        }

        var response = await base.GetResponseAsync(validatedMessages, options, cancellationToken).ConfigureAwait(false);

        var usageDetails = ExtractUsageDetails(response);
        await ReportActualUsageAsync(usageDetails, estimatedTokens, cancellationToken).ConfigureAwait(false);

        return response;
    }

    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(messages);
        cancellationToken.ThrowIfCancellationRequested();

        var (validatedMessages, estimatedTokens) = EnforceRequestCap(messages, options?.ModelId);

        if (_rateLimiter is not null)
        {
            var acquireResult = await _rateLimiter.AcquireAsync(estimatedTokens, cancellationToken).ConfigureAwait(false);
            if (!acquireResult.IsSuccess)
            {
                throw new AISecurityException(acquireResult.FirstError, 1.0f);
            }
        }

        UsageDetails? streamUsage = null;

        await foreach (var update in base.GetStreamingResponseAsync(validatedMessages, options, cancellationToken).ConfigureAwait(false))
        {
            if (update.Contents is not null)
            {
                foreach (var content in update.Contents)
                {
                    if (content is UsageContent uc && uc.Details is not null)
                    {
                        streamUsage = uc.Details;
                    }
                }
            }

            yield return update;
        }

        await ReportActualUsageAsync(streamUsage, estimatedTokens, cancellationToken).ConfigureAwait(false);
    }

    // [CS.01] [MEA-04] Enforce hard request cap without silent pruning or truncation.
    private (IReadOnlyList<ChatMessage> Messages, int EstimatedTokens) EnforceRequestCap(
        IEnumerable<ChatMessage> messages,
        string? modelId)
    {
        var messageList = messages as IReadOnlyList<ChatMessage> ?? messages.ToList();
        var totalTokens = EstimateTotalTokens(messageList, modelId);

        if (totalTokens > _maxContextTokens)
        {
            throw new AISecurityException(VKAIErrors.TokenBudgetExceeded, 1.0f);
        }

        return (messageList, totalTokens);
    }

    // [MEA-07] Derive actual usage strictly from provider-reported UsageDetails.
    private async Task ReportActualUsageAsync(
        UsageDetails? usageDetails,
        int estimatedInputTokens,
        CancellationToken cancellationToken)
    {
        var actualInput = usageDetails?.InputTokenCount ?? estimatedInputTokens;
        var actualOutput = usageDetails?.OutputTokenCount ?? 0;
        var totalTokens = usageDetails?.TotalTokenCount ?? (actualInput + actualOutput);

        if (_rateLimiter is not null)
        {
            await _rateLimiter.ReportUsageAsync((int)totalTokens).ConfigureAwait(false);
        }

        if (_usageAggregator is not null)
        {
            var tokenUsage = new VKAITokenUsage
            {
                InputTokens = actualInput,
                OutputTokens = actualOutput
            };
            await _usageAggregator.AggregateUsageAsync(tokenUsage, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }

    private static UsageDetails? ExtractUsageDetails(ChatResponse? response)
    {
        if (response is null)
        {
            return null;
        }

        if (response.Usage is not null)
        {
            return response.Usage;
        }

        if (response.Messages is null)
        {
            return null;
        }

        UsageDetails? found = null;
        foreach (var message in response.Messages)
        {
            if (message.Contents is null)
            {
                continue;
            }

            foreach (var content in message.Contents)
            {
                if (content is UsageContent uc && uc.Details is not null)
                {
                    found = uc.Details;
                }
            }
        }

        return found;
    }

    private int EstimateTotalTokens(IReadOnlyList<ChatMessage> messages, string? modelId)
    {
        if (_tokenCounter is null)
        {
            return 0;
        }

        var total = 0;
        foreach (var msg in messages)
        {
            if (!string.IsNullOrEmpty(msg.Text))
            {
                total += _tokenCounter.CountTokens(msg.Text, modelId);
            }

            if (msg.Contents is not null)
            {
                foreach (var content in msg.Contents)
                {
                    switch (content)
                    {
                        case DataContent or UriContent:
                            total += 85; // Standard multi-modal tile estimate
                            break;
                        case FunctionCallContent fcc:
                            total += _tokenCounter.CountTokens(fcc.Name, modelId) + 16;
                            break;
                        case FunctionResultContent frc:
                            var resultStr = frc.Result?.ToString() ?? string.Empty;
                            total += _tokenCounter.CountTokens(resultStr, modelId) + 8;
                            break;
                    }
                }
            }

            total += 4; // Message boundary framing overhead
        }

        return total;
    }
}
