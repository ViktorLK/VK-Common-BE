using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Compression.Internal;

/// <summary>
/// Compression strategy based on text summarization via chat engine.
/// </summary>
internal sealed class LlmSummaryCompressionStrategy : IVKCompressionStrategy
{
    private readonly IVKChatEngine _chatEngine;
    private readonly VKCompressionOptions _options;

    public LlmSummaryCompressionStrategy(
        IVKChatEngine chatEngine,
        IOptions<VKCompressionOptions> options)
    {
        _chatEngine = VKGuard.NotNull(chatEngine);
        _options = VKGuard.NotNull(options?.Value);
    }

    public async Task<VKResult<string>> CompressAsync(string content, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(content);

        if (string.IsNullOrWhiteSpace(content))
        {
            return VKResult.Success(string.Empty);
        }

        string prompt = $"Summarize the following chat history briefly, consolidating key points, facts, and preferences:\n\n{content}";
        var messages = new[] { VKChatMessage.FromText(VKChatRole.User, prompt) };

        IVKAIArgs? chatArgs = null;
        if (!string.IsNullOrWhiteSpace(_options.ModelId))
        {
            chatArgs = new VKChatArgs { ModelId = _options.ModelId };
        }

        try
        {
            var result = await _chatEngine.SendAsync(messages, chatArgs, cancellationToken).ConfigureAwait(false);
            if (!result.IsSuccess)
            {
                return VKResult.Failure<string>(result.FirstError);
            }

            return VKResult.Success(result.Value.Message.Content ?? string.Empty);
        }
        catch (Exception ex)
        {
            return VKResult.Failure<string>(new VKError("AI.Engram.Compression.LlmSummaryError", ex.Message));
        }
    }
}
