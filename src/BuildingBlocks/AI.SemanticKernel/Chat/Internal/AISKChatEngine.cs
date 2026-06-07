using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
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
using VK.Blocks.AI.SemanticKernel.Common.Diagnostics.Internal;
using VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;
using VK.Blocks.Core;

using VK.Blocks.AI.SemanticKernel.Common.DependencyInjection;

namespace VK.Blocks.AI.SemanticKernel.Chat.Internal;

/// <summary>
/// A pure Semantic Kernel implementation of <see cref="IVKChatEngine"/>.
/// </summary>
internal sealed class AISKChatEngine : AISKEngineBase<VKChatOptions>, IVKChatEngine
{


    private readonly IChatCompletionService _chatCompletion;

    public AISKChatEngine(
        Microsoft.SemanticKernel.Kernel kernel,
        IOptions<VKAIDefaultsOptions> globalOptions,
        IOptions<VKChatOptions> chatOptions,
        IOptions<VKAISKDefaultsOptions> aiskOptions,
        ILoggerFactory loggerFactory,
        TimeProvider? timeProvider = null)
        : base(kernel, globalOptions, chatOptions, loggerFactory.CreateLogger<AISKChatEngine>(), timeProvider)
    {
        _chatCompletion = new CompositeChatCompletionService(
            kernel,
            chatOptions,
            loggerFactory.CreateLogger<CompositeChatCompletionService>());
    }

    /// <inheritdoc />
    public Task<VKResult<VKChatResponse>> SendAsync(
        IEnumerable<VKChatMessage> messages,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        return SendAsyncInternal(messages, null, args, cancellationToken);
    }

    /// <inheritdoc />
    public Task<VKResult<VKChatResponse>> SendAsync(
        VKContextPayload payload,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(payload);
        return SendAsyncInternal(payload.Messages, payload, args, cancellationToken);
    }

    private Task<VKResult<VKChatResponse>> SendAsyncInternal(
        IEnumerable<VKChatMessage> messages,
        VKContextPayload? payload,
        IVKAIArgs? args,
        CancellationToken cancellationToken)
    {
        VKGuard.NotNull(messages);

        return ExecuteAsync(async (ct) =>
        {
            var stopwatch = Stopwatch.StartNew();

            // 0. Inject Dynamic Tools from Args (Rule 1: Abstracted Plugins)
            var overrides = args as IVKChatOverrides;
            var tools = overrides?.Tools;
            if (tools != null && tools.Count > 0)
            {
                if (!Kernel.Plugins.Contains("RequestTools"))
                {
                    var functions = tools.Select(VK.Blocks.AI.SemanticKernel.Agents.Internal.AISKAgentToolAdapter.ToKernelFunction).ToArray();
                    var plugin = Microsoft.SemanticKernel.KernelPluginFactory.CreateFromFunctions("RequestTools", functions);
                    Kernel.Plugins.Add(plugin);
                }
            }

            // 1. Convert VKChatMessages to SK ChatHistory
            ChatHistory chatHistory = AISKChatHistoryBuilder.Build(messages);

            if (FeatureOptions.EnablePromptLogging)
            {
                var sb = new System.Text.StringBuilder();
                foreach (var history in chatHistory)
                {
                    sb.AppendLine($"===================[{history.Role.Label}]===================");
                    foreach (var item in history.Items)
                    {
                        if (item is TextContent tc)
                            sb.AppendLine(tc.Text);
                    }
                    sb.AppendLine();
                }
                Logger.LogInformation("LLM Prompt:\n{Prompt}", sb.ToString());
            }

            // 2. Resolve Service
            IChatCompletionService chatService = GetChatService(args);

            // 3. Prepare Execution Settings
            PromptExecutionSettings executionSettings = CreateExecutionSettings(args);

            if (payload is { EnableContextCaching: true })
            {
                executionSettings.ExtensionData ??= new Dictionary<string, object>();
                executionSettings.ExtensionData["VKContextCacheKey"] = payload.ContextCacheKey;
                executionSettings.ExtensionData["EnableContextCaching"] = true;
            }

            // 4. Call SK Chat Completion
            IReadOnlyList<ChatMessageContent> result = await chatService.GetChatMessageContentsAsync(
                chatHistory,
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

            var message = new VKChatMessage
            {
                Role = VKChatRole.Assistant,
                Content = assistantMessage.Content ?? string.Empty,
                ModelId = assistantMessage.ModelId,
                Metadata = metadata,
                // Attempt to extract reasoning from metadata or specific items (connector dependent)
                ReasoningContent = metadata?.TryGetValue("Reasoning", out var r) == true ? r?.ToString() : null
            };

            // Map standard token usage structure
            VKAITokenUsage? aiUsage = usage;

            return new VKChatResponse
            {
                Message = message,
                Usage = aiUsage,
                FinishReason = metadata?.TryGetValue("FinishReason", out var fr) == true ? fr?.ToString() : null,
                Metadata = metadata
            };
        }, args, VKChatErrors.FeatureDisabled, cancellationToken);
    }
    private VKAITokenUsage? RecordObservability(ChatMessageContent message, IReadOnlyDictionary<string, object?>? metadata, double durationSeconds)
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

                return new VKAITokenUsage
                {
                    InputTokens = prompt,
                    OutputTokens = completion
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
        return SendStreamingAsyncInternal(messages, null, args, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<VKResult<VKChatStreamingResponse>> SendStreamingAsync(
        VKContextPayload payload,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(payload);
        return SendStreamingAsyncInternal(payload.Messages, payload, args, cancellationToken);
    }

    private IAsyncEnumerable<VKResult<VKChatStreamingResponse>> SendStreamingAsyncInternal(
        IEnumerable<VKChatMessage> messages,
        VKContextPayload? payload,
        IVKAIArgs? args,
        CancellationToken cancellationToken)
    {
        VKGuard.NotNull(messages);

        return ExecuteStreamingAsync(StreamInternal, args, VKChatErrors.FeatureDisabled, cancellationToken);

        async IAsyncEnumerable<VKChatStreamingResponse> StreamInternal([EnumeratorCancellation] CancellationToken ct)
        {
            // 0. Inject Dynamic Tools from Args
            var overrides = args as IVKChatOverrides;
            var tools = overrides?.Tools;
            if (tools != null && tools.Count > 0)
            {
                if (!Kernel.Plugins.Contains("RequestTools"))
                {
                    var functions = tools.Select(VK.Blocks.AI.SemanticKernel.Agents.Internal.AISKAgentToolAdapter.ToKernelFunction).ToArray();
                    var plugin = Microsoft.SemanticKernel.KernelPluginFactory.CreateFromFunctions("RequestTools", functions);
                    Kernel.Plugins.Add(plugin);
                }
            }

            // 1. Setup
            ChatHistory history = AISKChatHistoryBuilder.Build(messages);

            if (FeatureOptions.EnablePromptLogging)
            {
                var promptJson = System.Text.Json.JsonSerializer.Serialize(history, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                Logger.LogInformation("LLM Prompt [Streaming]:\n{Prompt}", promptJson);
            }

            IChatCompletionService chatService = GetChatService(args);
            PromptExecutionSettings executionSettings = CreateExecutionSettings(args);

            if (payload is { EnableContextCaching: true })
            {
                executionSettings.ExtensionData ??= new Dictionary<string, object>();
                executionSettings.ExtensionData["VKContextCacheKey"] = payload.ContextCacheKey;
                executionSettings.ExtensionData["EnableContextCaching"] = true;
            }

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
                                metadata["TokenUsage"] = new VKAITokenUsage
                                {
                                    InputTokens = usage.InputTokens ?? 0,
                                    OutputTokens = usage.OutputTokens ?? 0
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
        _ = args;
        return _chatCompletion;
    }

    /// <inheritdoc />
    public Task<VKResult<VKStructuredChatResponse<T>>> SendStructuredAsync<T>(
        IEnumerable<VKChatMessage> messages,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default) where T : class
    {
        VKGuard.NotNull(messages); // [AP.01]

        var provider = (args as IVKAIProviderOverrides)?.Provider?.ToString()
            ?? FeatureOptions.Provider?.ToString()
            ?? GlobalOptions.Provider.ToString();

        // Structured output (JSON Schema mode) is supported only by OpenAI-compatible providers.
        if (!string.Equals(provider, "OpenAI", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(provider, "AzureOpenAI", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(
                VKResult.Failure<VKStructuredChatResponse<T>>(
                    new VKError("AI.Chat.StructuredOutputNotSupported",
                        $"Provider '{provider}' does not support structured JSON output. Use OpenAI or AzureOpenAI.")));
        }

        return ExecuteAsync(async (ct) => // [CS.01]
        {
            var stopwatch = Stopwatch.StartNew();

            // 1. Build chat history
            ChatHistory chatHistory = AISKChatHistoryBuilder.Build(messages);

            // 2. Build execution settings with JSON Schema response format
            var chatArgs = args as VKChatArgs;
            var genArgs = args as IVKGenerationOptions;

            var settings = new OpenAIPromptExecutionSettings
            {
                Temperature = genArgs?.Temperature ?? FeatureOptions.Temperature,
                MaxTokens = genArgs?.MaxTokens ?? FeatureOptions.MaxTokens,
                TopP = genArgs?.TopP ?? FeatureOptions.TopP,
                ModelId = chatArgs?.ModelId ?? FeatureOptions.ModelId,
                User = args?.UserId,
                ResponseFormat = typeof(T) // SK resolves JSON schema from the CLR type
            };

            // 3. Call SK Chat Completion
            IChatCompletionService chatService = GetChatService(args);
            IReadOnlyList<ChatMessageContent> result = await chatService.GetChatMessageContentsAsync(
                chatHistory,
                settings,
                Kernel,
                ct).ConfigureAwait(false); // [CS.03]

            ChatMessageContent? assistantMessage = result.FirstOrDefault();
            if (assistantMessage is null)
            {
                throw new InvalidOperationException("No chat message content returned from the service.");
            }

            // 4. Deserialize the JSON content into T
            var rawContent = assistantMessage.Content;
            if (string.IsNullOrWhiteSpace(rawContent))
            {
                throw new InvalidOperationException("LLM returned empty content for structured output.");
            }

            T? deserializedData = JsonSerializer.Deserialize<T>(rawContent);

            if (deserializedData is null)
            {
                throw new Exception("Structured response deserialized to null.");
            }

            // 5. Observability
            var metadata = assistantMessage.Metadata?.ToDictionary(k => k.Key, v => v.Value)
                ?? new Dictionary<string, object?>();
            var usage = RecordObservability(assistantMessage, metadata, stopwatch.Elapsed.TotalSeconds);

            return new VKStructuredChatResponse<T>
            {
                Data = deserializedData,
                Usage = usage,
                ModelId = assistantMessage.ModelId,
                FinishReason = metadata.TryGetValue("FinishReason", out var fr) ? fr?.ToString() : null,
                Metadata = metadata
            };

        }, args, VKChatErrors.FeatureDisabled, cancellationToken); // [CS.01]
    }

    private PromptExecutionSettings CreateExecutionSettings(IVKAIArgs? args)
    {
        var genArgs = args as IVKGenerationOptions;
        var chatArgs = args as VKChatArgs;

        var provider = (args as IVKAIProviderOverrides)?.Provider?.ToString() ?? FeatureOptions.Provider?.ToString() ?? GlobalOptions.Provider.ToString();
        var enableAutoTool = chatArgs?.EnableAutoToolCalling ?? FeatureOptions.EnableAutoToolCalling;
        PromptExecutionSettings settings;

        if (string.Equals(provider, "Ollama", StringComparison.OrdinalIgnoreCase))
        {
            settings = new OllamaPromptExecutionSettings();
        }
        else if (string.Equals(provider, "Google", StringComparison.OrdinalIgnoreCase))
        {
            settings = new GeminiPromptExecutionSettings();
        }
        else
        {
            settings = new OpenAIPromptExecutionSettings();
        }

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

                // [Phase 1: Auto Function Calling] FunctionChoiceBehavior.Auto()
                if (enableAutoTool && Kernel.Plugins.Count > 0) // [CS.01]
                {
                    openAi.FunctionChoiceBehavior = FunctionChoiceBehavior.Auto();
                }
                break;
            case GeminiPromptExecutionSettings google:
                google.Temperature = genArgs?.Temperature ?? FeatureOptions.Temperature;
                google.MaxTokens = genArgs?.MaxTokens ?? FeatureOptions.MaxTokens;
                google.TopP = genArgs?.TopP ?? FeatureOptions.TopP;
                google.StopSequences = FeatureOptions.StopSequences.ToList();

                // [Phase 1: Auto Function Calling] FunctionChoiceBehavior.Auto()
                if (enableAutoTool && Kernel.Plugins.Count > 0) // [CS.01]
                {
                    google.FunctionChoiceBehavior = FunctionChoiceBehavior.Auto();
                }
                break;
            case OllamaPromptExecutionSettings ollama:
                ollama.Temperature = genArgs?.Temperature ?? FeatureOptions.Temperature;
                if (enableAutoTool && Kernel.Plugins.Count > 0)
                {
                    ollama.FunctionChoiceBehavior = FunctionChoiceBehavior.Auto();
                }
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

