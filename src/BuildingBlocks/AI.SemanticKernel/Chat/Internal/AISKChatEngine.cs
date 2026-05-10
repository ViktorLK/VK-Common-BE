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
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Connectors.OpenAI;
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
        ILogger<AISKChatEngine> logger)
        : base(kernel, globalOptions, chatOptions, logger)
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

        return ExecuteAsync(async () =>
        {
            // 1. Convert VKChatMessages to SK ChatHistory
            ChatHistory history = AISKChatHistoryBuilder.Build(messages);

            // 2. Resolve Service (support ServiceId/Keyed Service)
            // TODO: Implement high-level failover here (e.g., try alternative ServiceIds if primary fails)
            IChatCompletionService chatService = GetChatService(args);

            // 3. Prepare Execution Settings
            PromptExecutionSettings executionSettings = CreateExecutionSettings(args);

            // 4. Call SK Chat Completion
            IReadOnlyList<ChatMessageContent> result = await chatService.GetChatMessageContentsAsync(
                history,
                executionSettings,
                Kernel,
                cancellationToken).ConfigureAwait(false);

            // 5. Map Result back to VKChatMessage
            ChatMessageContent? assistantMessage = result.FirstOrDefault();
            if (assistantMessage is null)
            {
                throw new InvalidOperationException("No chat message content returned from the service.");
            }

            return new VKChatMessage
            {
                Role = VKChatRole.Assistant,
                Content = assistantMessage.Content ?? string.Empty,
                ModelId = assistantMessage.ModelId
            };
        });
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<VKResult<VKChatStreamingResponse>> SendStreamingAsync(
        IEnumerable<VKChatMessage> messages,
        IVKAIArgs? args = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(messages);

        ChatHistory history = AISKChatHistoryBuilder.Build(messages);
        IChatCompletionService chatService = GetChatService(args);
        PromptExecutionSettings executionSettings = CreateExecutionSettings(args);

        IAsyncEnumerable<StreamingChatMessageContent>? streamingResult = null;
        VKError? setupError = null;

        try
        {
            streamingResult = chatService.GetStreamingChatMessageContentsAsync(
                history,
                executionSettings,
                Kernel,
                cancellationToken);
        }
        catch (Exception ex)
        {
            setupError = AISKErrorMapper.Map(ex);
        }

        if (setupError is not null || streamingResult is null)
        {
            yield return VKResult.Failure<VKChatStreamingResponse>(setupError ?? VKChatErrors.ExecutionError);
            yield break;
        }

        // Use a manual enumerator to handle errors during iteration
        var enumerator = streamingResult.GetAsyncEnumerator(cancellationToken);
        VKError? iterationError = null;

        try
        {
            while (true)
            {
                StreamingChatMessageContent? chunk;
                try
                {
                    if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                        break;
                    chunk = enumerator.Current;
                }
                catch (Exception ex)
                {
                    iterationError = AISKErrorMapper.Map(ex);
                    break;
                }

                if (chunk?.Content is not null)
                {
                    yield return VKResult.Success(new VKChatStreamingResponse
                    {
                        Delta = chunk.Content,
                        Role = VKChatRole.Assistant,
                        ModelId = chunk.ModelId,
                        IsFinal = false
                    });
                }
            }
        }
        finally
        {
            await enumerator.DisposeAsync().ConfigureAwait(false);
        }

        if (iterationError is not null)
        {
            yield return VKResult.Failure<VKChatStreamingResponse>(iterationError);
        }
        else
        {
            yield return VKResult.Success(new VKChatStreamingResponse { IsFinal = true });
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
        IVKGenerationSettings? genArgs = args as IVKGenerationSettings;
        VKChatArgs? chatArgs = args as VKChatArgs;

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
        // settings.ExtensionData["Timeout"] = GetEffectiveTimeout(args);

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
