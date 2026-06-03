using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using VK.Blocks.AI.SemanticKernel.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Filters.Internal;

/// <summary>
/// A Semantic Kernel filter that intercepts prompt rendering and function execution
/// to apply VK.Blocks.AI Tokenics (Audit & Rate Limiting).
/// </summary>
internal sealed class AISKTokenicsFilter(
    ILogger<AISKTokenicsFilter> logger,
    IVKTokenRateLimiter? rateLimiter = null,
    IVKTokenUsageAggregator? aggregator = null,
    IVKTokenCounter? tokenizer = null) : IPromptRenderFilter, IFunctionInvocationFilter
{
    private readonly ILogger<AISKTokenicsFilter> _logger = VKGuard.NotNull(logger);
    private readonly IVKTokenRateLimiter? _rateLimiter = rateLimiter;
    private readonly IVKTokenUsageAggregator? _aggregator = aggregator;
    private readonly IVKTokenCounter? _tokenizer = tokenizer;

    /// <inheritdoc />
    public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
    {
        VKGuard.NotNull(context);
        VKGuard.NotNull(next);

        // Render the prompt first so we can estimate token usage
        await next(context).ConfigureAwait(false);

        var renderedPrompt = context.RenderedPrompt;
        if (!string.IsNullOrWhiteSpace(renderedPrompt))
        {
            // Estimate tokens. If tokenizer is available, use it; otherwise, rough estimate (Length / 4)
            int estimatedTokens = _tokenizer?.CountTokens(renderedPrompt) ?? (renderedPrompt.Length / 4);

            if (_rateLimiter != null)
            {
                var acquireResult = await _rateLimiter.AcquireAsync(estimatedTokens, context.CancellationToken).ConfigureAwait(false);
                if (acquireResult.IsFailure)
                {
                    // Rate limit exceeded or acquisition failed
                    throw new VKDomainException(acquireResult.FirstError.Code, acquireResult.FirstError.Description);
                }
            }
        }
    }

    /// <inheritdoc />
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        VKGuard.NotNull(context);
        VKGuard.NotNull(next);

        // Attempt to extract audit details from Arguments
        context.Arguments.TryGetValue("UserId", out var userIdObj);

        string? userId = userIdObj?.ToString();
        string action = $"{context.Function.PluginName}.{context.Function.Name}";

        // Log the AI Audit start
        _logger.LogChatAudit(action, userId, null);

        await next(context).ConfigureAwait(false);

        if (context.Result?.Metadata != null)
        {
            if (context.Result.Metadata.TryGetValue("Usage", out var usageObj) && usageObj != null)
            {
                try
                {
                    dynamic usage = usageObj;
                    int prompt = usage.InputTokens ?? 0;
                    int completion = usage.OutputTokens ?? 0;
                    int total = usage.TotalTokens ?? (prompt + completion);

                    // Report the actual total usage back to the rate limiter
                    if (_rateLimiter != null)
                    {
                        await _rateLimiter.ReportUsageAsync(total).ConfigureAwait(false);
                    }

                    if (_aggregator != null)
                    {
                        var tokenUsage = new VKAITokenUsage
                        {
                            InputTokens = prompt,
                            OutputTokens = completion
                        };
                        await _aggregator.AggregateUsageAsync(tokenUsage, userId).ConfigureAwait(false);
                    }
                }
                catch
                {
                    // Ignore usage parsing errors gracefully
                }
            }
        }
    }
}
