using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Chat.Internal;

/// <summary>
/// Context pruner implementing a sliding window approach that fits messages into a token limit
/// while unconditionally preserving System messages and maintaining chronological message order.
/// Adheres to CS.01, CS.03, and AP.01.
/// </summary>
internal sealed class DefaultContextPruner : IVKAIContextPruner
{
    private readonly IVKTokenCounter _tokenCounter;

    public DefaultContextPruner(IVKTokenCounter tokenCounter)
    {
        _tokenCounter = VKGuard.NotNull(tokenCounter);
    }

    public ValueTask<VKResult<IReadOnlyList<VKChatMessage>>> PruneAsync(
        IReadOnlyList<VKChatMessage> history,
        int tokenLimit,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (tokenLimit <= 0)
        {
            return ValueTask.FromResult(VKResult.Failure<IReadOnlyList<VKChatMessage>>(VKAIErrors.InvalidTokenLimit));
        }

        if (history is null || history.Count == 0)
        {
            return ValueTask.FromResult(VKResult.Success<IReadOnlyList<VKChatMessage>>([]));
        }

        // Calculate token count for each message
        var messageTokens = new int[history.Count];
        var totalTokens = 0;
        var systemTokens = 0;

        for (var i = 0; i < history.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var count = EstimateMessageTokens(history[i]);
            messageTokens[i] = count;
            totalTokens += count;

            if (history[i].Role == VKChatRole.System)
            {
                systemTokens += count;
            }
        }

        // If history already fits within the token limit, return as is
        if (totalTokens <= tokenLimit)
        {
            return ValueTask.FromResult(VKResult.Success(history));
        }

        // Available budget for non-system messages
        var remainingBudget = tokenLimit - systemTokens;
        var keptIndices = new HashSet<int>();

        // System messages are unconditionally preserved
        for (var i = 0; i < history.Count; i++)
        {
            if (history[i].Role == VKChatRole.System)
            {
                keptIndices.Add(i);
            }
        }

        // Scan non-system messages in reverse (most recent first) to fit within remaining budget
        for (var i = history.Count - 1; i >= 0; i--)
        {
            if (history[i].Role == VKChatRole.System)
            {
                continue;
            }

            var cost = messageTokens[i];
            if (remainingBudget - cost >= 0)
            {
                keptIndices.Add(i);
                remainingBudget -= cost;
            }
            else
            {
                // Cannot fit this older message; stop sliding window
                break;
            }
        }

        // Reconstruct pruned list in original chronological order
        var pruned = new List<VKChatMessage>(keptIndices.Count);
        for (var i = 0; i < history.Count; i++)
        {
            if (keptIndices.Contains(i))
            {
                pruned.Add(history[i]);
            }
        }

        return ValueTask.FromResult(VKResult.Success<IReadOnlyList<VKChatMessage>>(pruned));
    }

    private int EstimateMessageTokens(VKChatMessage message)
    {
        if (message.TokenCount > 0)
        {
            return message.TokenCount;
        }

        var text = message.Content;
        if (string.IsNullOrEmpty(text) && message.Parts.Count > 0)
        {
            var sb = new StringBuilder();
            foreach (var part in message.Parts)
            {
                switch (part)
                {
                    case VKTextPart tp when !string.IsNullOrEmpty(tp.Text):
                        sb.Append(tp.Text).Append(' ');
                        break;
                    case VKReasoningPart rp when !string.IsNullOrEmpty(rp.Reasoning):
                        sb.Append(rp.Reasoning).Append(' ');
                        break;
                }
            }
            text = sb.ToString();
        }

        var tokens = _tokenCounter.CountTokens(text ?? string.Empty, message.ModelId);

        // Account for non-text parts (e.g. images, audio, tool calls)
        if (message.Parts.Count > 0)
        {
            foreach (var part in message.Parts)
            {
                if (part is VKImagePart or VKAudioPart or VKFilePart)
                {
                    tokens += 85; // Standard multi-modal tile estimate
                }
            }
        }

        if (message.ToolCalls != null)
        {
            foreach (var tc in message.ToolCalls)
            {
                tokens += _tokenCounter.CountTokens(tc.Name, message.ModelId) + 16;
            }
        }

        // Message framing overhead (role, markers)
        return tokens + 4;
    }
}
