using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Guardrails.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Guardrails.Content.Internal;

/// <summary>
/// A decorator that wraps an <see cref="IVKEmbeddingsEngine"/> to enforce content governance and safety moderation.
/// </summary>
internal sealed class VKGovernanceEmbeddingsDecorator : IVKEmbeddingsEngine
{
    private readonly IVKEmbeddingsEngine _inner;
    private readonly IVKModerationEngine _moderator;
    private readonly IOptions<VKContentOptions> _options;
    private readonly ILogger<VKGovernanceEmbeddingsDecorator> _logger;

    public VKGovernanceEmbeddingsDecorator(
        IVKEmbeddingsEngine inner,
        IVKModerationEngine moderator,
        IOptions<VKContentOptions> options,
        ILogger<VKGovernanceEmbeddingsDecorator> logger)
    {
        _inner = VKGuard.NotNull(inner);
        _moderator = VKGuard.NotNull(moderator);
        _options = VKGuard.NotNull(options);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<VKEmbeddingsResponse>> GetEmbeddingsAsync(
        IEnumerable<string> inputs,
        VKEmbeddingsArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        if (_options.Value.EnableContentFilter != true)
        {
            return await _inner.GetEmbeddingsAsync(inputs, args, cancellationToken).ConfigureAwait(false);
        }

        // 1. Check Input Texts
        foreach (var text in inputs)
        {
            if (string.IsNullOrWhiteSpace(text))
                continue;

            var moderationResult = await _moderator.CheckContentAsync(text, cancellationToken).ConfigureAwait(false);
            if (!moderationResult.IsSuccess)
            {
                return VKResult.Failure<VKEmbeddingsResponse>(moderationResult.Errors);
            }

            if (moderationResult.Value.IsFlagged)
            {
                GuardrailsDiagnostics.ContentFlagged(_logger, moderationResult.Value.FlaggedReason);
                return VKResult.Failure<VKEmbeddingsResponse>(VKAIErrors.SafetyViolation(moderationResult.Value.FlaggedReason ?? "Inappropriate input content"));
            }
        }

        // 2. Call Inner Engine
        return await _inner.GetEmbeddingsAsync(inputs, args, cancellationToken).ConfigureAwait(false);
    }
}
