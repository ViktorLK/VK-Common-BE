using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Chat.Internal;

/// <summary>
/// A composite chat completion service that handles cross-provider fallback logic.
/// Wraps multiple <see cref="IChatCompletionService"/> instances registered in the Kernel.
/// </summary>
internal sealed class CompositeChatCompletionService : IChatCompletionService
{
    private readonly Microsoft.SemanticKernel.Kernel _kernel;
    private readonly VKChatOptions? _chatOptions;
    private readonly IReadOnlyList<VKChatFallbackConfig> _fallbacks;
    private readonly IVKAIProviderTracker _tracker;
    private readonly ILogger<CompositeChatCompletionService> _logger;
    private readonly ResiliencePipeline<IReadOnlyList<ChatMessageContent>> _pipeline;

    public CompositeChatCompletionService(
        Microsoft.SemanticKernel.Kernel kernel,
        IOptions<VKChatOptions> chatOptions,
        IVKAIProviderTracker tracker,
        ILogger<CompositeChatCompletionService> logger)
    {
        _kernel = VKGuard.NotNull(kernel);
        _chatOptions = chatOptions?.Value;
        _fallbacks = chatOptions?.Value?.ChatFallbacks ?? [];
        _tracker = VKGuard.NotNull(tracker);
        _logger = VKGuard.NotNull(logger);

        _pipeline = BuildPipeline();
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
                    int attemptIndex = 0;
                    ctx.Properties.TryGetValue(new ResiliencePropertyKey<int>("AttemptIndex"), out attemptIndex);

                    // Find the next available provider starting from current attempt sequence
                    int targetIndex = attemptIndex;
                    while (targetIndex <= _fallbacks.Count)
                    {
                        var config = GetProviderConfig(targetIndex);
                        if (config != null && _tracker.IsAvailable(config))
                        {
                            break;
                        }
                        targetIndex++;
                    }

                    if (targetIndex > _fallbacks.Count)
                    {
                        targetIndex = attemptIndex; // fallback to original attempt index as last resort
                    }

                    string serviceId = targetIndex == 0 ? "primary" : $"fallback_{targetIndex - 1}";
                    var service = _kernel.GetRequiredService<IChatCompletionService>(serviceId);

                    var currentSettings = ctx.Properties.GetValue(new ResiliencePropertyKey<PromptExecutionSettings?>("Settings"), null);

                    var activeConfig = GetProviderConfig(targetIndex);
                    if (targetIndex > 0 && currentSettings is not null && activeConfig is not null)
                    {
                        currentSettings.ModelId = activeConfig.ModelId;
                    }

                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                    try
                    {
                        if (activeConfig != null)
                        {
                            _tracker.RecordRequest(activeConfig);
                        }
                        var result = await service.GetChatMessageContentsAsync(chatHistory, currentSettings, kernel ?? _kernel, ctx.CancellationToken).ConfigureAwait(false);

                        if (activeConfig != null)
                        {
                            int tokens = GetTokenCount(result);
                            _tracker.RecordMetrics(activeConfig, tokens, stopwatch.Elapsed);
                        }
                        return result;
                    }
                    catch (Exception ex) when (IsTransientOrRateLimitError(ex))
                    {
                        if (activeConfig != null)
                        {
                            _tracker.MarkFailure(activeConfig, ex);
                        }
                        throw;
                    }
                },
                context).AsTask();
        }
        finally
        {
            ResilienceContextPool.Shared.Return(context);
        }
    }

    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Microsoft.SemanticKernel.Kernel? kernel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int attemptIndex = 0;
        int maxAttempts = _fallbacks.Count + 1;

        while (attemptIndex < maxAttempts)
        {
            int targetIndex = attemptIndex;
            while (targetIndex <= _fallbacks.Count)
            {
                var config = GetProviderConfig(targetIndex);
                if (config != null && _tracker.IsAvailable(config))
                {
                    break;
                }
                targetIndex++;
            }

            if (targetIndex > _fallbacks.Count)
            {
                targetIndex = attemptIndex;
            }

            string serviceId = targetIndex == 0 ? "primary" : $"fallback_{targetIndex - 1}";
            IChatCompletionService service;
            try
            {
                service = _kernel.GetRequiredService<IChatCompletionService>(serviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resolve chat service '{ServiceId}'.", serviceId);
                attemptIndex++;
                continue;
            }

            var currentSettings = executionSettings;
            var activeConfig = GetProviderConfig(targetIndex);
            if (targetIndex > 0 && currentSettings is not null && activeConfig is not null)
            {
                currentSettings.ModelId = activeConfig.ModelId;
            }

            IAsyncEnumerator<StreamingChatMessageContent>? enumerator = null;
            bool iterateSuccessful = false;
            bool yieldedAny = false;
            int totalTokens = 0;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                if (activeConfig != null)
                {
                    _tracker.RecordRequest(activeConfig);
                }

                var stream = service.GetStreamingChatMessageContentsAsync(chatHistory, currentSettings, kernel ?? _kernel, cancellationToken);
                enumerator = stream.GetAsyncEnumerator(cancellationToken);
            }
            catch (Exception ex) when (IsTransientOrRateLimitError(ex))
            {
                if (activeConfig != null)
                {
                    _tracker.MarkFailure(activeConfig, ex);
                }
                _logger.LogWarning(ex, "Streaming chat service attempt {Attempt} failed during initialization. Falling back.", attemptIndex + 1);
                attemptIndex++;
                continue;
            }

            try
            {
                while (true)
                {
                    bool hasNext = false;
                    try
                    {
                        hasNext = await enumerator.MoveNextAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex) when (IsTransientOrRateLimitError(ex))
                    {
                        if (activeConfig != null)
                        {
                            _tracker.MarkFailure(activeConfig, ex);
                        }

                        if (yieldedAny)
                        {
                            _logger.LogError(ex, "Streaming failed after yielding content. Cannot fallback safely without duplicate content.");
                            throw;
                        }
                        _logger.LogWarning(ex, "Streaming chat service attempt {Attempt} failed during iteration. Falling back.", attemptIndex + 1);
                        attemptIndex++;
                        break;
                    }

                    if (!hasNext)
                    {
                        iterateSuccessful = true;
                        break;
                    }

                    var chunk = enumerator.Current;
                    if (chunk.Metadata != null && chunk.Metadata.TryGetValue("Usage", out var usageObj) && usageObj is not null)
                    {
                        try
                        {
                            dynamic usage = usageObj;
                            int prompt = usage.InputTokens ?? 0;
                            int completion = usage.OutputTokens ?? 0;
                            totalTokens = prompt + completion;
                        }
                        catch { }
                    }
                    else if (!string.IsNullOrEmpty(chunk.Content))
                    {
                        totalTokens += (chunk.Content.Length + 3) / 4;
                    }

                    yield return chunk;
                    yieldedAny = true;
                }
            }
            finally
            {
                if (enumerator is not null)
                {
                    await enumerator.DisposeAsync().ConfigureAwait(false);
                }
            }

            if (iterateSuccessful)
            {
                if (activeConfig != null)
                {
                    _tracker.RecordMetrics(activeConfig, totalTokens, stopwatch.Elapsed);
                }
                break;
            }
        }
    }

    private IVKAIProviderOptions? GetProviderConfig(int attemptIndex)
    {
        if (attemptIndex == 0)
        {
            return _chatOptions;
        }
        if (attemptIndex - 1 < _fallbacks.Count)
        {
            return _fallbacks[attemptIndex - 1];
        }
        return null;
    }

    private int GetTokenCount(IReadOnlyList<ChatMessageContent> results)
    {
        var msg = results?.FirstOrDefault();
        if (msg?.Metadata != null && msg.Metadata.TryGetValue("Usage", out var usageObj) && usageObj is not null)
        {
            try
            {
                dynamic usage = usageObj;
                int prompt = usage.InputTokens ?? 0;
                int completion = usage.OutputTokens ?? 0;
                return prompt + completion;
            }
            catch { }
        }
        return 0;
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
                    int currentAttempt = 0;
                    args.Context.Properties.TryGetValue(new ResiliencePropertyKey<int>("AttemptIndex"), out currentAttempt);
                    _logger.LogWarning(args.Outcome.Exception, "Chat service attempt {Attempt} failed. Falling back to next service.", currentAttempt + 1);
                    args.Context.Properties.Set(new ResiliencePropertyKey<int>("AttemptIndex"), currentAttempt + 1);
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
