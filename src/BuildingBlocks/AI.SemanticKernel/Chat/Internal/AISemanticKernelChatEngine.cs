using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

namespace VK.Blocks.AI.SemanticKernel.Chat.Internal;

/// <summary>
/// A pure Semantic Kernel implementation of <see cref="IVKChatEngine"/>.
/// </summary>
internal sealed class AISemanticKernelChatEngine : AISemanticKernelEngineBase<VKChatOptions>, IVKChatEngine
{
    private readonly IChatCompletionService _chatCompletion;

    public AISemanticKernelChatEngine(
        Microsoft.SemanticKernel.Kernel kernel,
        IOptions<VKAIOptions> globalOptions,
        IOptions<VKChatOptions> chatOptions,
        IOptions<VKAISemanticKernelOptions> AISemanticKernelOptions,
        ILoggerFactory loggerFactory,
        TimeProvider? timeProvider = null)
        : base(kernel, globalOptions, chatOptions, loggerFactory.CreateLogger<AISemanticKernelChatEngine>(), timeProvider)
    {
        _ = AISemanticKernelOptions;
        _chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
    }

    /// <inheritdoc />
    public Task<VKResult<VKChatResponse>> SendAsync(
        IEnumerable<VKChatMessage> messages,
        VKChatArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        return SendAsyncInternal(messages, null, args, cancellationToken);
    }

    /// <inheritdoc />
    public Task<VKResult<VKChatResponse>> SendAsync(
        VKContextPayload payload,
        VKChatArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(payload);
        return SendAsyncInternal(payload.Messages, payload, args, cancellationToken);
    }

    private Task<VKResult<VKChatResponse>> SendAsyncInternal(
        IEnumerable<VKChatMessage> messages,
        VKContextPayload? payload,
        VKChatArgs? args,
        CancellationToken cancellationToken)
    {
        VKGuard.NotNull(messages);

        return ExecuteAsync(async (ct) =>
        {
            var stopwatch = Stopwatch.StartNew();

            // 0. Inject Dynamic Tools from Args (Rule 1: Abstracted Plugins)
            var tools = args?.Tools;
            if (tools is not null && tools.Count > 0)
            {
                if (!Kernel.Plugins.Contains("RequestTools"))
                {
                    var functions = tools.Select(VK.Blocks.AI.SemanticKernel.Agents.Internal.AISemanticKernelAgentToolAdapter.ToKernelFunction).ToArray();
                    var plugin = Microsoft.SemanticKernel.KernelPluginFactory.CreateFromFunctions("RequestTools", functions);
                    Kernel.Plugins.Add(plugin);
                }
            }

            // 1. Convert VKChatMessages to SK ChatHistory
            ChatHistory chatHistory = AISemanticKernelChatHistoryBuilder.Build(messages);

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

            // Map Metadata
            var metadata = assistantMessage.Metadata?.ToDictionary(k => k.Key, v => v.Value) ?? new Dictionary<string, object?>();

            // Record Duration & Tokens & Logs
            var usage = RecordObservability(assistantMessage, metadata, stopwatch.Elapsed.TotalSeconds);
            if (usage is not null)
            {
                metadata["TokenUsage"] = usage;
            }

            var toolCalls = assistantMessage.Items
                .OfType<FunctionCallContent>()
                .Select(f => new VKToolCall
                {
                    Id = f.Id ?? string.Empty,
                    Name = f.FunctionName,
                    Arguments = f.Arguments?.ToDictionary(k => k.Key, v => v.Value ?? string.Empty) ?? new Dictionary<string, object>()
                })
                .ToList();

            var message = new VKChatMessage
            {
                Role = VKChatRole.Assistant,
                Content = assistantMessage.Content ?? string.Empty,
                ModelId = assistantMessage.ModelId,
                Metadata = metadata,
                ToolCalls = toolCalls.Count > 0 ? toolCalls : null,
                ReasoningContent = metadata?.TryGetValue("Reasoning", out var r) == true ? r?.ToString() : null
            };

            return new VKChatResponse
            {
                Message = message,
                Usage = usage,
                FinishReason = metadata?.TryGetValue("FinishReason", out var fr) == true ? fr?.ToString() : null,
                Metadata = metadata
            };
        }, VKChatErrors.FeatureDisabled, cancellationToken);
    }

    private VKAITokenUsage? RecordObservability(ChatMessageContent message, IReadOnlyDictionary<string, object?>? metadata, double durationSeconds)
    {
        AISemanticKernelMetrics.RecordChatDuration(durationSeconds, message.ModelId);

        if (metadata is null)
            return null;

        if (metadata.TryGetValue("Usage", out var usageObj) && usageObj is not null)
        {
            try
            {
                dynamic usage = usageObj;
                int prompt = usage.InputTokens ?? 0;
                int completion = usage.OutputTokens ?? 0;
                int total = usage.TotalTokens ?? (prompt + completion);

                Logger.LogTokenUsage(message.ModelId, prompt, completion, total);
                AISemanticKernelMetrics.RecordTokenUsage(message.ModelId, prompt, completion);

                return new VKAITokenUsage
                {
                    InputTokens = prompt,
                    OutputTokens = completion
                };
            }
            catch { }
        }

        return null;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<VKResult<VKChatStreamingResponse>> SendStreamingAsync(
        IEnumerable<VKChatMessage> messages,
        VKChatArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        return SendStreamingAsyncInternal(messages, null, args, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<VKResult<VKChatStreamingResponse>> SendStreamingAsync(
        VKContextPayload payload,
        VKChatArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(payload);
        return SendStreamingAsyncInternal(payload.Messages, payload, args, cancellationToken);
    }

    private IAsyncEnumerable<VKResult<VKChatStreamingResponse>> SendStreamingAsyncInternal(
        IEnumerable<VKChatMessage> messages,
        VKContextPayload? payload,
        VKChatArgs? args,
        CancellationToken cancellationToken)
    {
        VKGuard.NotNull(messages);

        return ExecuteStreamingAsync(StreamInternal, VKChatErrors.FeatureDisabled, cancellationToken);

        async IAsyncEnumerable<VKChatStreamingResponse> StreamInternal([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
        {
            // 0. Inject Dynamic Tools from Args
            var tools = args?.Tools;
            if (tools is not null && tools.Count > 0)
            {
                if (!Kernel.Plugins.Contains("RequestTools"))
                {
                    var functions = tools.Select(VK.Blocks.AI.SemanticKernel.Agents.Internal.AISemanticKernelAgentToolAdapter.ToKernelFunction).ToArray();
                    var plugin = Microsoft.SemanticKernel.KernelPluginFactory.CreateFromFunctions("RequestTools", functions);
                    Kernel.Plugins.Add(plugin);
                }
            }

            ChatHistory history = AISemanticKernelChatHistoryBuilder.Build(messages);
            IChatCompletionService chatService = GetChatService(args);
            PromptExecutionSettings executionSettings = CreateExecutionSettings(args);

            if (payload is { EnableContextCaching: true })
            {
                executionSettings.ExtensionData ??= new Dictionary<string, object>();
                executionSettings.ExtensionData["VKContextCacheKey"] = payload.ContextCacheKey;
                executionSettings.ExtensionData["EnableContextCaching"] = true;
            }

            var channel = Channel.CreateUnbounded<VKChatStreamingResponse>(new UnboundedChannelOptions
            {
                SingleWriter = true,
                SingleReader = true
            });

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

                        if (metadata.TryGetValue("Usage", out var usageObj) && usageObj is not null)
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

                    await channel.Writer.WriteAsync(new VKChatStreamingResponse
                    {
                        IsFinal = true,
                        Metadata = lastMetadata
                    }, ct).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    channel.Writer.TryComplete(ex);
                    return;
                }
                finally
                {
                    channel.Writer.TryComplete();
                }
            }, ct);

            while (await channel.Reader.WaitToReadAsync(ct).ConfigureAwait(false))
            {
                while (channel.Reader.TryRead(out var response))
                {
                    yield return response;
                }
            }
        }
    }

    private IChatCompletionService GetChatService(VKChatArgs? args)
    {
        _ = args;
        return _chatCompletion;
    }

    /// <inheritdoc />
    public Task<VKResult<VKStructuredChatResponse<T>>> SendStructuredAsync<T>(
        IEnumerable<VKChatMessage> messages,
        VKChatArgs? args = null,
        CancellationToken cancellationToken = default) where T : class
    {
        VKGuard.NotNull(messages);

        var provider = args?.Provider?.ToString() ?? FeatureOptions.Provider?.ToString() ?? "OpenAI";

        if (!string.Equals(provider, "OpenAI", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(provider, "AzureOpenAI", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(provider, "Google", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(
                VKResult.Failure<VKStructuredChatResponse<T>>(
                    new VKError("AI.Chat.StructuredOutputNotSupported",
                        $"Provider '{provider}' does not support structured JSON output. Use OpenAI, AzureOpenAI, or Google.")));
        }

        return ExecuteAsync(async (ct) =>
        {
            var stopwatch = Stopwatch.StartNew();

            ChatHistory chatHistory = AISemanticKernelChatHistoryBuilder.Build(messages);

            PromptExecutionSettings settings;
            if (string.Equals(provider, "Google", StringComparison.OrdinalIgnoreCase))
            {
                var schemaNode = System.Text.Json.Schema.JsonSchemaExporter.GetJsonSchemaAsNode(JsonSerializerOptions.Default, typeof(T));
                var schemaString = schemaNode.ToString();

                settings = new GeminiPromptExecutionSettings
                {
                    Temperature = args?.Temperature ?? FeatureOptions.Temperature,
                    MaxTokens = args?.MaxTokens ?? FeatureOptions.MaxTokens,
                    TopP = args?.TopP ?? FeatureOptions.TopP,
                    ModelId = args?.ModelId ?? FeatureOptions.ModelId,
                    ResponseMimeType = "application/json",
                    ResponseSchema = KernelJsonSchema.Parse(schemaString)
                };
            }
            else
            {
                settings = new OpenAIPromptExecutionSettings
                {
                    Temperature = args?.Temperature ?? FeatureOptions.Temperature,
                    MaxTokens = args?.MaxTokens ?? FeatureOptions.MaxTokens,
                    TopP = args?.TopP ?? FeatureOptions.TopP,
                    ModelId = args?.ModelId ?? FeatureOptions.ModelId,
                    ResponseFormat = typeof(T)
                };
            }

            IChatCompletionService chatService = GetChatService(args);
            IReadOnlyList<ChatMessageContent> result = await chatService.GetChatMessageContentsAsync(
                chatHistory,
                settings,
                Kernel,
                ct).ConfigureAwait(false);

            ChatMessageContent? assistantMessage = result.FirstOrDefault();
            if (assistantMessage is null)
            {
                throw new InvalidOperationException("No chat message content returned from the service.");
            }

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

        }, VKChatErrors.FeatureDisabled, cancellationToken);
    }

    private PromptExecutionSettings CreateExecutionSettings(VKChatArgs? args)
    {
        var provider = args?.Provider?.ToString() ?? FeatureOptions.Provider?.ToString() ?? "OpenAI";
        var autoInvoke = args?.AutoInvokeTools ?? FeatureOptions.AutoInvokeTools ?? true;
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

        switch (settings)
        {
            case OpenAIPromptExecutionSettings openAi:
                openAi.Temperature = args?.Temperature ?? FeatureOptions.Temperature;
                openAi.MaxTokens = args?.MaxTokens ?? FeatureOptions.MaxTokens;
                openAi.TopP = args?.TopP ?? FeatureOptions.TopP;
                openAi.FrequencyPenalty = args?.FrequencyPenalty ?? FeatureOptions.FrequencyPenalty;
                openAi.PresencePenalty = args?.PresencePenalty ?? FeatureOptions.PresencePenalty;
                openAi.StopSequences = (args?.StopSequences ?? FeatureOptions.StopSequences)?.ToList();

                var openAiBehavior = ResolveFunctionChoiceBehavior(args, autoInvoke, Kernel.Plugins.Count);
                if (openAiBehavior is not null)
                {
                    openAi.FunctionChoiceBehavior = openAiBehavior;
                }
                break;
            case GeminiPromptExecutionSettings google:
                google.Temperature = args?.Temperature ?? FeatureOptions.Temperature;
                google.MaxTokens = args?.MaxTokens ?? FeatureOptions.MaxTokens;
                google.TopP = args?.TopP ?? FeatureOptions.TopP;
                google.StopSequences = (args?.StopSequences ?? FeatureOptions.StopSequences)?.ToList();

                var googleBehavior = ResolveFunctionChoiceBehavior(args, autoInvoke, Kernel.Plugins.Count);
                if (googleBehavior is not null)
                {
                    google.FunctionChoiceBehavior = googleBehavior;
                }
                break;
            case OllamaPromptExecutionSettings ollama:
                ollama.Temperature = args?.Temperature ?? FeatureOptions.Temperature;

                var ollamaBehavior = ResolveFunctionChoiceBehavior(args, autoInvoke, Kernel.Plugins.Count);
                if (ollamaBehavior is not null)
                {
                    ollama.FunctionChoiceBehavior = ollamaBehavior;
                }
                break;
        }

        settings.ModelId = args?.ModelId ?? FeatureOptions.ModelId;

        return settings;
    }

    private static FunctionChoiceBehavior? ResolveFunctionChoiceBehavior(VKChatArgs? chatArgs, bool autoInvoke, int pluginCount)
    {
        if (chatArgs is not null && !string.IsNullOrWhiteSpace(chatArgs.ToolChoice))
        {
            var choice = chatArgs.ToolChoice.Trim();

            if (string.Equals(choice, "None", StringComparison.OrdinalIgnoreCase))
            {
                return FunctionChoiceBehavior.None();
            }

            if (string.Equals(choice, "Auto", StringComparison.OrdinalIgnoreCase))
            {
                return FunctionChoiceBehavior.Auto(autoInvoke: autoInvoke);
            }

            if (string.Equals(choice, "Required", StringComparison.OrdinalIgnoreCase))
            {
                return FunctionChoiceBehavior.Required(autoInvoke: autoInvoke);
            }

            return FunctionChoiceBehavior.Required(autoInvoke: autoInvoke);
        }

        if (pluginCount > 0)
        {
            return FunctionChoiceBehavior.Auto(autoInvoke: autoInvoke);
        }

        return null;
    }
}
