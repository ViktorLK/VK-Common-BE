using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI;
using VK.Blocks.AI.Cognitive.Memory.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Memory.Internal;

internal sealed class DefaultMemorySummarizer : IVKMemorySummarizer
{
    private readonly IVKChatEngine _chatEngine;
    private readonly ILogger<DefaultMemorySummarizer> _logger;

    public DefaultMemorySummarizer(
        IVKChatEngine chatEngine,
        ILogger<DefaultMemorySummarizer> logger)
    {
        _chatEngine = VKGuard.NotNull(chatEngine);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<string>> SummarizeAsync(
        string content,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(content);

        string prompt = $"Summarize the following conversation context briefly while preserving key information:\n\n{content}";
        var messages = new[] { VKChatMessage.FromText(VKChatRole.User, prompt) };

        try
        {
            var result = await _chatEngine.SendAsync(messages, null, cancellationToken).ConfigureAwait(false);

            if (!result.IsSuccess)
            {
                return VKResult.Failure<string>(result.FirstError);
            }

            return VKResult.Success(result.Value.Message.Content ?? string.Empty);
        }
        catch (Exception ex)
        {
            MemoryDiagnostics.MemorySummarizationFailed(_logger, ex);
            return VKResult.Failure<string>(new VKError("Cognitive.Memory.SummarizationError", ex.Message));
        }
    }
}
