using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Retry;
using VK.Blocks.AI.SemanticKernel.Common.Diagnostics.Internal;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Chat.Internal;

/// <summary>
/// A composite chat completion service that handles cross-provider fallback logic.
/// Wraps multiple <see cref="IChatCompletionService"/> instances registered in the Kernel.
/// </summary>
internal sealed class CompositeChatCompletionService : IChatCompletionService
{
    private readonly Microsoft.SemanticKernel.Kernel _kernel;
    private readonly IReadOnlyList<VKChatFallbackConfig> _fallbacks;
    private readonly ILogger<CompositeChatCompletionService> _logger;
    private readonly ResiliencePipeline<IReadOnlyList<ChatMessageContent>> _pipeline;
    private readonly ResiliencePipeline<IAsyncEnumerable<StreamingChatMessageContent>> _streamingPipeline;

    public CompositeChatCompletionService(
        Microsoft.SemanticKernel.Kernel kernel,
        IOptions<VKChatOptions> chatOptions,
        ILogger<CompositeChatCompletionService> logger)
    {
        _kernel = VKGuard.NotNull(kernel);
        _fallbacks = chatOptions?.Value?.ChatFallbacks ?? [];
        _logger = VKGuard.NotNull(logger);

        _pipeline = BuildPipeline();
        _streamingPipeline = BuildStreamingPipeline();
    }

    public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

    public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Microsoft.SemanticKernel.Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        var context = ResilienceContextPool.Shared.Get(cancellationToken);

        // Pass the settings so we can mutate the ModelId during fallback
        context.Properties.Set(new ResiliencePropertyKey<PromptExecutionSettings?>("Settings"), executionSettings);

        try
        {
            return _pipeline.ExecuteAsync(
                async (ctx) =>
                {
                    // The "state" tells us which service index to use: -1 is primary, 0+ are fallbacks
                    int attemptIndex = 0;
                    ctx.Properties.TryGetValue(new ResiliencePropertyKey<int>("AttemptIndex"), out attemptIndex);

                    string serviceId = attemptIndex == 0 ? "primary" : $"fallback_{attemptIndex - 1}";

                    var service = _kernel.GetRequiredService<IChatCompletionService>(serviceId);

                    var currentSettings = ctx.Properties.GetValue(new ResiliencePropertyKey<PromptExecutionSettings?>("Settings"), null);

                    // Mutate ModelId if this is a fallback attempt
                    if (attemptIndex > 0 && currentSettings is not null)
                    {
                        var fallbackConfig = _fallbacks[attemptIndex - 1];
                        currentSettings.ModelId = fallbackConfig.ModelId;
                    }

                    return await service.GetChatMessageContentsAsync(chatHistory, currentSettings, kernel ?? _kernel, ctx.CancellationToken).ConfigureAwait(false);
                },
                context).AsTask();
        }
        finally
        {
            ResilienceContextPool.Shared.Return(context);
        }
    }

    public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Microsoft.SemanticKernel.Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        // Polly v8 streaming fallback is complex because we must return an IAsyncEnumerable.
        // For streaming, we will build a dedicated pipeline or manually handle the iteration fallback.
        // To keep it simple and robust, we use a custom enumerator wrapper or execute it within the pipeline.

        var context = ResilienceContextPool.Shared.Get(cancellationToken);
        context.Properties.Set(new ResiliencePropertyKey<PromptExecutionSettings?>("Settings"), executionSettings);

        try
        {
            var result = _streamingPipeline.Execute(
                (ctx) =>
                {
                    int attemptIndex = 0;
                    ctx.Properties.TryGetValue(new ResiliencePropertyKey<int>("AttemptIndex"), out attemptIndex);

                    string serviceId = attemptIndex == 0 ? "primary" : $"fallback_{attemptIndex - 1}";
                    var service = _kernel.GetRequiredService<IChatCompletionService>(serviceId);
                    var currentSettings = ctx.Properties.GetValue(new ResiliencePropertyKey<PromptExecutionSettings?>("Settings"), null);

                    if (attemptIndex > 0 && currentSettings is not null)
                    {
                        var fallbackConfig = _fallbacks[attemptIndex - 1];
                        currentSettings.ModelId = fallbackConfig.ModelId;
                    }

                    return service.GetStreamingChatMessageContentsAsync(chatHistory, currentSettings, kernel ?? _kernel, ctx.CancellationToken);
                },
                context);

            return result;
        }
        finally
        {
            ResilienceContextPool.Shared.Return(context);
        }
    }

    private ResiliencePipeline<IReadOnlyList<ChatMessageContent>> BuildPipeline()
    {
        var builder = new ResiliencePipelineBuilder<IReadOnlyList<ChatMessageContent>>();

        if (_fallbacks.Count > 0)
        {
            builder.AddRetry(new RetryStrategyOptions<IReadOnlyList<ChatMessageContent>>
            {
                MaxRetryAttempts = _fallbacks.Count,
                Delay = TimeSpan.Zero,
                ShouldHandle = new PredicateBuilder<IReadOnlyList<ChatMessageContent>>()
                    .Handle<Exception>(IsTransientOrRateLimitError),
                OnRetry = args =>
                {
                    _logger.LogWarning(args.Outcome.Exception, "Chat service attempt {Attempt} failed. Falling back to next service.", args.AttemptNumber);
                    args.Context.Properties.Set(new ResiliencePropertyKey<int>("AttemptIndex"), args.AttemptNumber + 1);
                    return default;
                }
            });
        }

        return builder.Build();
    }

    private ResiliencePipeline<IAsyncEnumerable<StreamingChatMessageContent>> BuildStreamingPipeline()
    {
        var builder = new ResiliencePipelineBuilder<IAsyncEnumerable<StreamingChatMessageContent>>();

        if (_fallbacks.Count > 0)
        {
            builder.AddRetry(new RetryStrategyOptions<IAsyncEnumerable<StreamingChatMessageContent>>
            {
                MaxRetryAttempts = _fallbacks.Count,
                Delay = TimeSpan.Zero,
                ShouldHandle = new PredicateBuilder<IAsyncEnumerable<StreamingChatMessageContent>>()
                    .Handle<Exception>(IsTransientOrRateLimitError),
                OnRetry = args =>
                {
                    _logger.LogWarning(args.Outcome.Exception, "Streaming chat service attempt {Attempt} failed. Falling back to next service.", args.AttemptNumber);
                    args.Context.Properties.Set(new ResiliencePropertyKey<int>("AttemptIndex"), args.AttemptNumber + 1);
                    return default;
                }
            });
        }

        return builder.Build();
    }

    private bool IsTransientOrRateLimitError(Exception ex)
    {
        if (ex is BrokenCircuitException)
            return true;

        // Match 429 Too Many Requests
        if (ex is HttpOperationException httpEx && httpEx.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            return true;

        // Check inner exceptions (e.g. from HttpClient)
        if (ex.InnerException is BrokenCircuitException)
            return true;

        return false;
    }
}
