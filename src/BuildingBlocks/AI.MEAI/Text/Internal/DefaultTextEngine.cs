using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.AI.MEAI.Common.Shared;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI.Text.Internal;

/// <summary>
/// Adapts Microsoft.Extensions.AI IChatClient to the pure VK.Blocks.AI IVKTextEngine port.
/// Adheres to CS.01, CS.03.
/// </summary>
internal sealed class DefaultTextEngine(
    IChatClient chatClient,
    IServiceProvider serviceProvider) : IVKTextEngine
{
    private readonly IChatClient _defaultChatClient = VKGuard.NotNull(chatClient);
    private readonly IServiceProvider _serviceProvider = VKGuard.NotNull(serviceProvider);

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

    public async Task<VKResult<VKTextResponse>> GenerateAsync(
        string prompt,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(prompt);

        try
        {
            var options = BuildOptions(args);
            var client = ResolveChatClient((args as IVKAIProviderOptions)?.Provider);
            var response = await client.GetResponseAsync(prompt, options, cancellationToken).ConfigureAwait(false);
            var content = response.Text ?? string.Empty;

            var res = new VKTextResponse
            {
                Text = content,
                FinishReason = response.FinishReason?.ToString(),
                Usage = response.Usage is not null
                    ? new VKAITokenUsage
                    {
                        InputTokens = (int)response.Usage.InputTokenCount.GetValueOrDefault(),
                        OutputTokens = (int)response.Usage.OutputTokenCount.GetValueOrDefault()
                    }
                    : null
            };

            return VKResult.Success(res);
        }
        catch (Exception ex)
        {
            return AIMEAIErrorMapper.MapToResult<VKTextResponse>(ex);
        }
    }

    public async IAsyncEnumerable<VKResult<VKTextResponse>> GenerateStreamingAsync(
        string prompt,
        IVKAIArgs? args = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(prompt);

        var options = BuildOptions(args);
        IAsyncEnumerator<ChatResponseUpdate>? enumerator = null;
        Exception? caughtInitException = null;

        try
        {
            var client = ResolveChatClient((args as IVKAIProviderOptions)?.Provider);
            enumerator = client.GetStreamingResponseAsync(prompt, options, cancellationToken)
                .GetAsyncEnumerator(cancellationToken);
        }
        catch (Exception ex)
        {
            caughtInitException = ex;
        }

        if (caughtInitException is not null)
        {
            yield return AIMEAIErrorMapper.MapToResult<VKTextResponse>(caughtInitException);
            yield break;
        }

        if (enumerator is null)
        {
            yield break;
        }

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
                    yield return AIMEAIErrorMapper.MapToResult<VKTextResponse>(moveException);
                    yield break;
                }

                if (!hasNext)
                {
                    break;
                }

                if (!string.IsNullOrEmpty(update.Text))
                {
                    yield return VKResult.Success(new VKTextResponse
                    {
                        Text = update.Text
                    });
                }
            }
        }
        finally
        {
            await enumerator.DisposeAsync().ConfigureAwait(false);
        }
    }

    private static ChatOptions? BuildOptions(IVKAIArgs? args)
    {
        if (args is VKTextArgs textArgs)
        {
            var options = new ChatOptions
            {
                Temperature = textArgs.Temperature,
                MaxOutputTokens = textArgs.MaxTokens,
                TopP = textArgs.TopP,
                FrequencyPenalty = textArgs.FrequencyPenalty,
                PresencePenalty = textArgs.PresencePenalty,
                ModelId = textArgs.ModelId
            };

            if (textArgs.StopSequences is not null && textArgs.StopSequences.Count > 0)
            {
                options.StopSequences = System.Linq.Enumerable.ToList(textArgs.StopSequences);
            }

            return options;
        }

        return null;
    }
}
