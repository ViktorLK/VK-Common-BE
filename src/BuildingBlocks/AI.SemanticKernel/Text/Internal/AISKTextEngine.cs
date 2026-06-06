using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;
using VK.Blocks.AI.SemanticKernel.Common.Diagnostics.Internal;
using VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Text.Internal;

/// <summary>
/// A Semantic Kernel implementation of <see cref="IVKTextEngine"/>.
/// </summary>
internal sealed class AISKTextEngine : AISKEngineBase<VKTextOptions>, IVKTextEngine
{
    private readonly ITextGenerationService? _textGeneration;
    private readonly IChatCompletionService? _chatCompletion;

    public AISKTextEngine(
        Microsoft.SemanticKernel.Kernel kernel,
        IOptions<VKAIDefaultsOptions> globalOptions,
        IOptions<VKTextOptions> textOptions,
        ILogger<AISKTextEngine> logger,
        TimeProvider? timeProvider = null)
        : base(kernel, globalOptions, textOptions, logger, timeProvider)
    {
        _textGeneration = kernel.Services.GetServices<ITextGenerationService>().LastOrDefault();
        _chatCompletion = kernel.Services.GetServices<IChatCompletionService>().LastOrDefault();

        if (_textGeneration == null && _chatCompletion == null)
        {
            throw new InvalidOperationException("No AI service (Text or Chat) registered in the kernel.");
        }
    }

    /// <inheritdoc />
    public Task<VKResult<VKTextResponse>> GenerateAsync(
        string prompt,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(prompt);

        return ExecuteAsync(async (ct) =>
        {
            var stopwatch = Stopwatch.StartNew();
            PromptExecutionSettings executionSettings = CreateExecutionSettings(args);

            string contentText;
            string? modelId = null;
            VKAITokenUsage? aiUsage = null;
            IDictionary<string, object?>? metadata = null;

            if (_textGeneration != null)
            {
                var result = await _textGeneration.GetTextContentsAsync(prompt, executionSettings, Kernel, ct).ConfigureAwait(false);
                if (result.Count > 0)
                {
                    var textContent = result[0];
                    contentText = textContent.Text ?? string.Empty;
                    modelId = textContent.ModelId;
                    metadata = textContent.Metadata?.ToDictionary(k => k.Key, v => v.Value) ?? new Dictionary<string, object?>();

                    if (metadata.TryGetValue("Usage", out var usageObj) && usageObj != null)
                    {
                        try
                        {
                            dynamic usage = usageObj;
                            aiUsage = new VKAITokenUsage
                            {
                                InputTokens = usage.InputTokens ?? 0,
                                OutputTokens = usage.OutputTokens ?? 0
                            };
                        }
                        catch { }
                    }
                }
                else
                {
                    contentText = string.Empty;
                }
            }
            else
            {
                // Fallback to Chat
                var chat = _chatCompletion!;
                var history = new ChatHistory();
                history.AddUserMessage(prompt);
                var result = await chat.GetChatMessageContentAsync(history, executionSettings, Kernel, ct).ConfigureAwait(false);
                contentText = result.Content ?? string.Empty;
                modelId = result.ModelId;
                metadata = result.Metadata?.ToDictionary(k => k.Key, v => v.Value) ?? new Dictionary<string, object?>();

                if (metadata.TryGetValue("Usage", out var usageObj) && usageObj != null)
                {
                    try
                    {
                        dynamic usage = usageObj;
                        aiUsage = new VKAITokenUsage
                        {
                            InputTokens = usage.InputTokens ?? 0,
                            OutputTokens = usage.OutputTokens ?? 0
                        };
                    }
                    catch { }
                }
            }

            // Record Observability
            RecordObservability(modelId, stopwatch.Elapsed.TotalSeconds);

            return new VKTextResponse
            {
                Text = contentText,
                ModelId = modelId,
                Usage = aiUsage,
                Metadata = metadata
            };
        }, args, VKTextErrors.FeatureDisabled, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<VKResult<VKTextResponse>> GenerateStreamingAsync(
        string prompt,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(prompt);

        return ExecuteStreamingAsync(StreamInternal, args, VKTextErrors.FeatureDisabled, cancellationToken);

        async IAsyncEnumerable<VKTextResponse> StreamInternal([EnumeratorCancellation] CancellationToken ct)
        {
            PromptExecutionSettings executionSettings = CreateExecutionSettings(args);

            if (_textGeneration != null)
            {
                var streamingResult = _textGeneration.GetStreamingTextContentsAsync(prompt, executionSettings, Kernel, ct);
                await foreach (var chunk in streamingResult.WithCancellation(ct).ConfigureAwait(false))
                {
                    if (chunk?.Text is not null)
                    {
                        var metadata = chunk.Metadata?.ToDictionary(k => k.Key, v => v.Value) ?? new Dictionary<string, object?>();
                        VKAITokenUsage? aiUsage = null;
                        if (metadata.TryGetValue("Usage", out var usageObj) && usageObj != null)
                        {
                            try
                            {
                                dynamic usage = usageObj;
                                aiUsage = new VKAITokenUsage
                                {
                                    InputTokens = usage.InputTokens ?? 0,
                                    OutputTokens = usage.OutputTokens ?? 0
                                };
                            }
                            catch { }
                        }

                        yield return new VKTextResponse
                        {
                            Text = chunk.Text,
                            ModelId = chunk.ModelId,
                            Usage = aiUsage,
                            Metadata = metadata
                        };
                    }
                }
            }
            else
            {
                var chat = _chatCompletion!;
                var history = new ChatHistory();
                history.AddUserMessage(prompt);
                var streamingResult = chat.GetStreamingChatMessageContentsAsync(history, executionSettings, Kernel, ct);
                await foreach (var chunk in streamingResult.WithCancellation(ct).ConfigureAwait(false))
                {
                    if (chunk?.Content is not null)
                    {
                        var metadata = chunk.Metadata?.ToDictionary(k => k.Key, v => v.Value) ?? new Dictionary<string, object?>();
                        VKAITokenUsage? aiUsage = null;
                        if (metadata.TryGetValue("Usage", out var usageObj) && usageObj != null)
                        {
                            try
                            {
                                dynamic usage = usageObj;
                                aiUsage = new VKAITokenUsage
                                {
                                    InputTokens = usage.InputTokens ?? 0,
                                    OutputTokens = usage.OutputTokens ?? 0
                                };
                            }
                            catch { }
                        }

                        yield return new VKTextResponse
                        {
                            Text = chunk.Content,
                            ModelId = chunk.ModelId,
                            Usage = aiUsage,
                            Metadata = metadata
                        };
                    }
                }
            }
        }
    }

    private PromptExecutionSettings CreateExecutionSettings(IVKAIArgs? args)
    {
        IVKGenerationOptions? genArgs = args as IVKGenerationOptions;

        // Use default settings
        var settings = new PromptExecutionSettings
        {
            ModelId = (args as IVKAIProviderOverrides)?.ModelId ?? FeatureOptions.ModelId
        };

        // Apply common parameters if provided
        if (genArgs != null)
        {
            settings.ExtensionData ??= new Dictionary<string, object>();
            settings.ExtensionData["temperature"] = genArgs.Temperature ?? FeatureOptions.Temperature ?? 0.7f;
            settings.ExtensionData["max_tokens"] = genArgs.MaxTokens ?? FeatureOptions.MaxTokens ?? 512;
            if (genArgs.TopP.HasValue)
                settings.ExtensionData["top_p"] = genArgs.TopP.Value;
        }

        return settings;
    }

    private void RecordObservability(string? modelId, double durationSeconds)
    {
        AISKMetrics.RecordChatDuration(durationSeconds, modelId); // Reuse chat duration for now or add specific text metrics
        Logger.LogTextGenerationCompleted(modelId, durationSeconds);
    }
}
