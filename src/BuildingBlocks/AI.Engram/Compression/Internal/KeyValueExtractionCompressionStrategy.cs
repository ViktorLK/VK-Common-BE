using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Compression.Internal;

/// <summary>
/// Compression strategy based on key-value fact extraction.
/// </summary>
internal sealed class KeyValueExtractionCompressionStrategy : IVKCompressionStrategy
{
    private readonly IVKChatEngine _chatEngine;
    private readonly VKCompressionOptions _options;

    public KeyValueExtractionCompressionStrategy(
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
            return VKResult.Success("[]");
        }

        string prompt = "Extract key facts, user preferences, and important context details from the following conversation history.\n" +
                        "Format the output strictly as a JSON array of objects containing 'topic' and 'fact' fields. " +
                        "Do not include any markdown formatting (like ```json), explanations, or intro/outro text. Just return the raw JSON.\n\n" +
                        $"CONVERSATION HISTORY:\n{content}";

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

            string responseContent = result.Value.Message.Content ?? "[]";
            
            // Basic sanitization to clean up model output wrap
            responseContent = responseContent.Trim();
            if (responseContent.StartsWith("```"))
            {
                int firstLineEnd = responseContent.IndexOf('\n');
                if (firstLineEnd != -1)
                {
                    responseContent = responseContent[firstLineEnd..].Trim();
                }
                if (responseContent.EndsWith("```"))
                {
                    responseContent = responseContent[..^3].Trim();
                }
            }

            return VKResult.Success(responseContent);
        }
        catch (Exception ex)
        {
            return VKResult.Failure<string>(new VKError("AI.Engram.Compression.KeyValueExtractionError", ex.Message));
        }
    }
}
