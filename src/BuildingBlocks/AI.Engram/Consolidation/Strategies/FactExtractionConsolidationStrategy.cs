using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Consolidation.Strategies;

/// <summary>
/// Consolidation strategy based on fact extraction using an LLM.
/// </summary>
internal sealed class FactExtractionConsolidationStrategy : IVKConsolidationStrategy
{
    private readonly IVKChatEngine _chatEngine;

    public FactExtractionConsolidationStrategy(IVKChatEngine chatEngine)
    {
        _chatEngine = VKGuard.NotNull(chatEngine);
    }

    public async Task<VKResult<string>> ConsolidateAsync(string[] contents, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(contents);

        if (contents.Length == 0)
        {
            return VKResult.Success(string.Empty);
        }

        var contentsJoined = string.Join("\n", contents);
        var prompt = $"Extract key facts, entities, and relationships from the following conversation context. Represent them as a bulleted list of facts. Avoid generalities and focus on specific details:\n\n{contentsJoined}";
        var messages = new[] { VKChatMessage.FromText(VKChatRole.User, prompt) };

        try
        {
            var result = await _chatEngine.SendAsync(messages, null, cancellationToken).ConfigureAwait(false); // [CS.03]

            if (!result.IsSuccess)
            {
                return VKResult.Failure<string>(result.Errors); // [CS.01]
            }

            var content = result.Value.Message.Content ?? string.Empty;
            return VKResult.Success(content); // [CS.01]
        }
        catch (Exception ex)
        {
            return VKResult.Failure<string>(new VKError(VKConsolidationErrors.FactExtractionFailed.Code, ex.Message)); // [CS.01]
        }
    }
}
