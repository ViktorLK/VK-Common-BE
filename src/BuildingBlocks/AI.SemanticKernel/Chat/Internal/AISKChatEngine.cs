using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using VK.Blocks.AI.SemanticKernel.Diagnostics.Internal;
using VK.Blocks.AI.SemanticKernel.Kernel.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Chat.Internal;

/// <summary>
/// A pure Semantic Kernel implementation of <see cref="IVKChatEngine"/>.
/// </summary>
internal sealed class AISKChatEngine : AISKEngineBase<VKChatOptions>, IVKChatEngine
{
    private readonly IChatCompletionService _chatCompletion;

    public AISKChatEngine(
        Microsoft.SemanticKernel.Kernel kernel,
        IOptions<VKAIOptions> globalOptions,
        IOptions<VKChatOptions> chatOptions,
        ILogger<AISKChatEngine> logger,
        TimeProvider? timeProvider = null)
        : base(kernel, globalOptions, chatOptions, logger, timeProvider)
    {
        _chatCompletion = GetService<IChatCompletionService>();
    }

    /// <inheritdoc />
    public Task<VKResult<VKChatMessage>> SendAsync(
        IEnumerable<VKChatMessage> messages,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(messages);

        return ExecuteAsync(async (ct) =>
        {
            var stopwatch = Stopwatch.StartNew();

            // 1. Convert VKChatMessages to SK ChatHistory
            ChatHistory history = AISKChatHistoryBuilder.Build(messages);

            // 2. Resolve Service
            IChatCompletionService chatService = GetChatService(args);

            // 3. Prepare Execution Settings
            PromptExecutionSettings executionSettings = CreateExecutionSettings(args);

            // 4. Call SK Chat Completion
            IReadOnlyList<ChatMessageContent> result = await chatService.GetChatMessageContentsAsync(
                history,
                executionSettings,
                Kernel,
                ct).ConfigureAwait(false);

            // 5. Map Result back to VKChatMessage
            ChatMessageContent? assistantMessage = result.FirstOrDefault();
            if (assistantMessage is null)
            {
                throw new InvalidOperationException("No chat message content returned from the service.");
            }

            // --- Industrial DNA: Observability & Metadata ---

            // Map Metadata
            var metadata = assistantMessage.Metadata?.ToDictionary(k => k.Key, v => v.Value) ?? new Dictionary<string, object?>();

            // Record Duration & Tokens & Logs
            var usage = RecordObservability(assistantMessage, metadata, stopwatch.Elapsed.TotalSeconds);
            if (usage != null)
            {
                metadata["TokenUsage"] = usage;
            }

            // Audit
            if (GetEffectiveEnableAudit())
            {
                Logger.LogChatAudit("SendAsync", args?.UserId, assistantMessage.ModelId);
            }

            return new VKChatMessage
            {
                Role = VKChatRole.Assistant,
                Content = assistantMessage.Content ?? string.Empty,
                ModelId = assistantMessage.ModelId,
                Metadata = metadata,
                // Attempt to extract reasoning from metadata or specific items (connector dependent)
                ReasoningContent = metadata?.TryGetValue("Reasoning", out var r) == true ? r?.ToString() : null
            };
        }, args, VKChatErrors.FeatureDisabled, cancellationToken);
    }

    private VKTokenUsage? RecordObservability(ChatMessageContent message, IDictionary<string, object?>? metadata, double durationSeconds)
    {
        // 1. Record Duration
        AISKMetrics.RecordChatDuration(durationSeconds, message.ModelId);

        if (metadata == null)
            return null;

        // 2. Extract and Record Token Usage (OpenAI/Standard pattern)
        if (metadata.TryGetValue("Usage", out var usageObj) && usageObj != null)
        {
            try
            {
                dynamic usage = usageObj;
                int prompt = usage.InputTokens ?? 0;
                int completion = usage.OutputTokens ?? 0;
                int total = usage.TotalTokens ?? (prompt + completion);

                // Log
                Logger.LogTokenUsage(message.ModelId, prompt, completion, total);

                // Metric
                AISKMetrics.RecordTokenUsage(message.ModelId, prompt, completion);

                return new VKTokenUsage
                {
                    PromptTokens = prompt,
                    CompletionTokens = completion
                };
            }
            catch { /* Best effort: Metadata format varies by connector */ }
        }

        return null;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<VKResult<VKChatStreamingResponse>> SendStreamingAsync(
        IEnumerable<VKChatMessage> messages,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(messages);

        return ExecuteStreamingAsync(StreamInternal, args, VKChatErrors.FeatureDisabled, cancellationToken);

        async IAsyncEnumerable<VKChatStreamingResponse> StreamInternal([EnumeratorCancellation] CancellationToken ct)
        {
            // 1. Setup
            ChatHistory history = AISKChatHistoryBuilder.Build(messages);
            IChatCompletionService chatService = GetChatService(args);
            PromptExecutionSettings executionSettings = CreateExecutionSettings(args);

            // Audit
            if (GetEffectiveEnableAudit())
            {
                Logger.LogChatAudit("SendStreamingAsync", args?.UserId, executionSettings.ModelId);
            }

            // 2. High-Performance Channel Buffer (Rule 4: Performance)
            // Use SingleReader/SingleWriter for optimization.
            var channel = Channel.CreateUnbounded<VKChatStreamingResponse>(new System.Threading.Channels.UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = true,
                AllowSynchronousContinuations = true
            });

            // Fire-and-forget producer task to fill the channel
            _ = Task.Run(async () =>
            {
                IDictionary<string, object?>? lastMetadata = null;
                try
                {
                    IAsyncEnumerable<StreamingChatMessageContent> streamingResult = chatService.GetStreamingChatMessageContentsAsync(
                        history,
                        executionSettings,
                        Kernel,
                        ct);

                    await foreach (var chunk in streamingResult.WithCancellation(ct).ConfigureAwait(false))
                    {
                        string? reasoningDelta = null;
                        if (chunk.Metadata?.TryGetValue("Reasoning", out var r) == true)
                        {
                            reasoningDelta = r?.ToString();
                        }

                        var metadata = chunk.Metadata?.ToDictionary(k => k.Key, v => v.Value) ?? new Dictionary<string, object?>();

                        if (metadata.TryGetValue("Usage", out var usageObj) && usageObj != null)
                        {
                            try
                            {
                                dynamic usage = usageObj;
                                metadata["TokenUsage"] = new VKTokenUsage
                                {
                                    PromptTokens = usage.InputTokens ?? 0,
                                    CompletionTokens = usage.OutputTokens ?? 0
                                };
                            }
                            catch { }
                        }

                        lastMetadata = metadata;

                        if (chunk?.Content is not null || reasoningDelta is not null || metadata.ContainsKey("TokenUsage"))
                        {
                            await channel.Writer.WriteAsync(new VKChatStreamingResponse
                            {
                                Delta = chunk?.Content ?? string.Empty,
                                ReasoningDelta = reasoningDelta,
                                Role = VKChatRole.Assistant,
                                ModelId = chunk?.ModelId,
                                IsFinal = false,
                                Metadata = metadata
                            }, ct).ConfigureAwait(false);
                        }
                    }

                    // Yield final signal
                    await channel.Writer.WriteAsync(new VKChatStreamingResponse
                    {
                        IsFinal = true,
                        Metadata = lastMetadata
                    }, ct).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // For performance, we don't want to swallow errors here,
                    // but they will be handled by the consumer's ExecuteStreamingAsync wrapper
                    // if we rethrow or close with error.
                    channel.Writer.TryComplete(ex);
                    return;
                }
                finally
                {
                    channel.Writer.TryComplete();
                }
            }, ct);

            // 3. Consumer: Read from Channel
            while (await channel.Reader.WaitToReadAsync(ct).ConfigureAwait(false))
            {
                while (channel.Reader.TryRead(out var response))
                {
                    yield return response;
                }
            }
        }
    }

    private IChatCompletionService GetChatService(IVKAIArgs? args)
    {
        if (args is VKChatArgs chatArgs && !string.IsNullOrWhiteSpace(chatArgs.ServiceId))
        {
            return GetService<IChatCompletionService>(chatArgs.ServiceId);
        }

        return _chatCompletion;
    }

    private PromptExecutionSettings CreateExecutionSettings(IVKAIArgs? args)
    {
        var genArgs = args as IVKGenerationSettings;
        var chatArgs = args as VKChatArgs;

        // Use OpenAI as the default settings base
        PromptExecutionSettings settings = new OpenAIPromptExecutionSettings();

        // Apply settings using pattern matching
        switch (settings)
        {
            case OpenAIPromptExecutionSettings openAi:
                openAi.Temperature = genArgs?.Temperature ?? FeatureOptions.Temperature;
                openAi.MaxTokens = genArgs?.MaxTokens ?? FeatureOptions.MaxTokens;
                openAi.TopP = genArgs?.TopP ?? FeatureOptions.TopP;
                openAi.FrequencyPenalty = FeatureOptions.FrequencyPenalty;
                openAi.PresencePenalty = FeatureOptions.PresencePenalty;
                openAi.StopSequences = FeatureOptions.StopSequences.ToList();
                openAi.User = args?.UserId;
                break;
            case GeminiPromptExecutionSettings google:
                google.Temperature = genArgs?.Temperature ?? FeatureOptions.Temperature;
                google.MaxTokens = genArgs?.MaxTokens ?? FeatureOptions.MaxTokens;
                google.TopP = genArgs?.TopP ?? FeatureOptions.TopP;
                google.StopSequences = FeatureOptions.StopSequences.ToList();
                break;
            case OllamaPromptExecutionSettings ollama:
                ollama.Temperature = genArgs?.Temperature ?? FeatureOptions.Temperature;
                break;
        }

        // 1. ModelId Override (Priority: Args -> Feature Options)
        settings.ModelId = chatArgs?.ModelId ?? FeatureOptions.ModelId;

        // 2. Resilience Overrides (Timeout integration is handled at the SK HttpClient level,
        // but we can pass it here for custom logic if needed).

        // 3. Advanced Context Bag
        if (chatArgs is { Context.Count: > 0 })
        {
            settings.ExtensionData ??= new Dictionary<string, object>();
            foreach (var kvp in chatArgs.Context)
            {
                settings.ExtensionData[kvp.Key] = kvp.Value;
            }
        }

        return settings;
    }
}
