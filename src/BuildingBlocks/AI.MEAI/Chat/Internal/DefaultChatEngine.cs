using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.MEAI.Common.Shared;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI.Chat.Internal;

/// <summary>
/// Adapts Microsoft.Extensions.AI IChatClient to the pure VK.Blocks.AI IVKChatEngine port.
/// Adheres to CS.01, CS.03, CS.06, AP.01, and maps all VKChatArgs options, tool calls, and token usage.
/// </summary>
internal sealed class DefaultChatEngine(
    IChatClient chatClient,
    IVKJsonSerializer jsonSerializer,
    IServiceProvider serviceProvider,
    IOptions<VKAIMEAIOptions>? extensionsOptions = null,
    IOptions<VKChatOptions>? chatOptions = null,
    IEnumerable<IVKAtomicTool>? registeredTools = null,
    IEnumerable<IVKAtomicToolFilter>? toolFilters = null) : IVKChatEngine
{
    private readonly IChatClient _defaultChatClient = VKGuard.NotNull(chatClient);
    private readonly IVKJsonSerializer _jsonSerializer = VKGuard.NotNull(jsonSerializer);
    private readonly IServiceProvider _serviceProvider = VKGuard.NotNull(serviceProvider);
    private readonly IOptions<VKAIMEAIOptions>? _extensionsOptions = extensionsOptions;
    private readonly IOptions<VKChatOptions>? _chatOptions = chatOptions;
    private readonly IEnumerable<IVKAtomicTool> _registeredTools = registeredTools ?? [];
    private readonly IEnumerable<IVKAtomicToolFilter> _toolFilters = toolFilters ?? [];

    private IChatClient ResolveChatClient(VKAIProviderType? provider)
    {
        if (provider.HasValue)
        {
            return _serviceProvider.GetKeyedService<IChatClient>(provider.Value)
                ?? _serviceProvider.GetKeyedService<IChatClient>(provider.Value.ToString())
                ?? _defaultChatClient;
        }
        return _defaultChatClient;
    }

    public async Task<VKResult<VKChatResponse>> SendAsync(
        IEnumerable<VKChatMessage> messages,
        VKChatArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(messages);

        try
        {
            var chatMessages = ChatMessageMapper.Map(messages);
            var options = BuildChatOptions(args);

            var client = ResolveChatClient(args?.Provider);
            var response = await client.GetResponseAsync(chatMessages, options, cancellationToken).ConfigureAwait(false);
            var result = ChatMessageMapper.MapToVKResponse(response);

            return VKResult.Success(result);
        }
        catch (Exception ex)
        {
            return AIMEAIErrorMapper.MapToResult<VKChatResponse>(ex);
        }
    }

    public async IAsyncEnumerable<VKResult<VKChatStreamingResponse>> SendStreamingAsync(
        IEnumerable<VKChatMessage> messages,
        VKChatArgs? args = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(messages);

        var chatMessages = ChatMessageMapper.Map(messages);
        var options = BuildChatOptions(args);

        IAsyncEnumerator<ChatResponseUpdate>? enumerator = null;
        Exception? caughtInitException = null;

        try
        {
            var client = ResolveChatClient(args?.Provider);
            enumerator = client.GetStreamingResponseAsync(chatMessages, options, cancellationToken)
                .GetAsyncEnumerator(cancellationToken);
        }
        catch (Exception ex)
        {
            caughtInitException = ex;
        }

        if (caughtInitException is not null)
        {
            yield return AIMEAIErrorMapper.MapToResult<VKChatStreamingResponse>(caughtInitException);
            yield break;
        }

        if (enumerator is null)
        {
            yield break;
        }

        bool finalChunkEmitted = false;

        try
        {
            while (true)
            {
                bool hasNext;
                ChatResponseUpdate update;
                Exception? moveException = null;

                try
                {
                    hasNext = await enumerator.MoveNextAsync().ConfigureAwait(false);
                    update = hasNext ? enumerator.Current : null!;
                }
                catch (Exception ex)
                {
                    moveException = ex;
                    hasNext = false;
                    update = null!;
                }

                if (moveException is not null)
                {
                    yield return AIMEAIErrorMapper.MapToResult<VKChatStreamingResponse>(moveException);
                    yield break;
                }

                if (!hasNext)
                {
                    break;
                }

                string? reasoningDelta = null;
                VKAITokenUsage? usage = null;

                if (update.Contents is not null)
                {
                    foreach (var content in update.Contents)
                    {
                        if (content is TextReasoningContent trc && !string.IsNullOrEmpty(trc.Text))
                        {
                            reasoningDelta = reasoningDelta is null ? trc.Text : reasoningDelta + trc.Text;
                        }
                        else if (content is UsageContent uc && uc.Details is not null)
                        {
                            usage = new VKAITokenUsage
                            {
                                InputTokens = uc.Details.InputTokenCount.GetValueOrDefault(),
                                OutputTokens = uc.Details.OutputTokenCount.GetValueOrDefault()
                            };
                        }
                    }
                }

                bool isFinal = update.FinishReason is not null;
                if (isFinal)
                {
                    finalChunkEmitted = true;
                }

                var hasText = !string.IsNullOrEmpty(update.Text);
                var hasReasoning = !string.IsNullOrEmpty(reasoningDelta);

                if (hasText || hasReasoning || isFinal || usage is not null)
                {
                    yield return VKResult.Success(new VKChatStreamingResponse
                    {
                        Delta = update.Text ?? string.Empty,
                        Role = VKChatRole.Assistant,
                        ModelId = update.ModelId,
                        ReasoningDelta = reasoningDelta,
                        IsFinal = isFinal,
                        Usage = usage,
                        Metadata = update.AdditionalProperties is not null
                            ? new Dictionary<string, object?>(update.AdditionalProperties)
                            : null
                    });
                }
            }

            if (!finalChunkEmitted)
            {
                yield return VKResult.Success(new VKChatStreamingResponse
                {
                    Delta = string.Empty,
                    Role = VKChatRole.Assistant,
                    IsFinal = true
                });
            }
        }
        finally
        {
            await enumerator.DisposeAsync().ConfigureAwait(false);
        }
    }

    public async Task<VKResult<VKChatStructuredResponse<T>>> SendStructuredAsync<T>(
        IEnumerable<VKChatMessage> messages,
        VKChatArgs? args = null,
        CancellationToken cancellationToken = default) where T : class
    {
        VKGuard.NotNull(messages);

        try
        {
            var chatMessages = ChatMessageMapper.Map(messages);
            var options = BuildChatOptions(args) ?? new ChatOptions();

            if (options.ResponseFormat is null)
            {
                var schema = AIJsonUtilities.CreateJsonSchema(typeof(T));
                options.ResponseFormat = ChatResponseFormat.ForJsonSchema(schema, schemaName: typeof(T).Name);
            }


            var client = ResolveChatClient(args?.Provider);
            var response = await client.GetResponseAsync(chatMessages, options, cancellationToken).ConfigureAwait(false);
            var text = response.Text;

            if (string.IsNullOrWhiteSpace(text))
            {
                return VKResult.Failure<VKChatStructuredResponse<T>>(VKAIErrors.EmptyResponse);
            }

            T? data;
            try
            {
                data = _jsonSerializer.Deserialize<T>(text);
            }
            catch (Exception ex)
            {
                return VKResult.Failure<VKChatStructuredResponse<T>>(
                    VKAIErrors.StructuredDeserializationFailed($"Failed to deserialize JSON into {typeof(T).Name}: {ex.Message}"));
            }

            if (data is null)
            {
                return VKResult.Failure<VKChatStructuredResponse<T>>(
                    VKAIErrors.StructuredDeserializationFailed($"Deserialization produced null for type {typeof(T).Name}."));
            }

            var usage = response.Usage is not null
                ? new VKAITokenUsage
                {
                    InputTokens = response.Usage.InputTokenCount.GetValueOrDefault(),
                    OutputTokens = response.Usage.OutputTokenCount.GetValueOrDefault()
                }
                : null;

            var metadata = response.AdditionalProperties is not null
                ? new Dictionary<string, object?>(response.AdditionalProperties)
                : null;

            var structured = new VKChatStructuredResponse<T>
            {
                Data = data,
                ModelId = response.ModelId,
                FinishReason = response.FinishReason?.ToString(),
                Usage = usage,
                Metadata = metadata
            };

            return VKResult.Success(structured);
        }
        catch (Exception ex)
        {
            return AIMEAIErrorMapper.MapToResult<VKChatStructuredResponse<T>>(ex);
        }
    }

    private ChatOptions? BuildChatOptions(VKChatArgs? args)
        => ChatOptionsTranslator.Translate(
            args,
            _chatOptions?.Value,
            _extensionsOptions?.Value,
            _registeredTools,
            _toolFilters);
}
