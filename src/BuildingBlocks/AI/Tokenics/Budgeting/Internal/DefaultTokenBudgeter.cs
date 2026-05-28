using System;
using System.Collections.Generic;
using System.Linq;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Tokenics.Budgeting.Internal;

/// <summary>
/// Default implementation of <see cref="IVKTokenBudgeter"/>.
/// </summary>
internal sealed class DefaultTokenBudgeter : IVKTokenBudgeter
{
    private readonly IVKTokenCounter _tokenCounter;

    public DefaultTokenBudgeter(IVKTokenCounter tokenizer)
    {
        _tokenCounter = VKGuard.NotNull(tokenizer);
    }

    /// <inheritdoc />
    public int GetRemainingBudget(IEnumerable<VKChatMessage> history, int maxTokens, string? modelId = null)
    {
        int usedTokens = history.Sum(m => _tokenCounter.CountTokens(m.Content, modelId));
        return Math.Max(0, maxTokens - usedTokens);
    }

    /// <inheritdoc />
    public IEnumerable<VKChatMessage> ApplyBudget(
        IEnumerable<VKChatMessage> history,
        int budget,
        VKTokenBudgetStrategy strategy = VKTokenBudgetStrategy.OldestFirst,
        string? modelId = null)
    {
        var messages = history.ToList();
        int currentTotal = messages.Sum(m => _tokenCounter.CountTokens(m.Content, modelId));

        if (currentTotal <= budget)
        {
            return messages;
        }

        return strategy switch
        {
            VKTokenBudgetStrategy.OldestFirst => TruncateOldestFirst(messages, budget, modelId),
            VKTokenBudgetStrategy.Error => throw new VKValidationException(VKAIErrors.ContextWindowExceeded.Description),
            _ => TruncateOldestFirst(messages, budget, modelId)
        };
    }

    private List<VKChatMessage> TruncateOldestFirst(List<VKChatMessage> messages, int budget, string? modelId)
    {
        // Keep System Prompt if present
        var systemPrompt = messages.FirstOrDefault(m => m.Role == VKChatRole.System);
        var others = messages.Where(m => m.Role != VKChatRole.System).ToList();

        int systemTokens = systemPrompt is not null ? _tokenCounter.CountTokens(systemPrompt.Content, modelId) : 0;
        int remainingBudget = budget - systemTokens;

        if (remainingBudget <= 0)
        {
            return systemPrompt is not null ? [systemPrompt] : [];
        }

        var result = new List<VKChatMessage>();
        int currentTokens = 0;

        // Iterate from newest to oldest
        for (int i = others.Count - 1; i >= 0; i--)
        {
            int tokens = _tokenCounter.CountTokens(others[i].Content, modelId);
            if (currentTokens + tokens <= remainingBudget)
            {
                result.Insert(0, others[i]);
                currentTokens += tokens;
            }
            else
            {
                break;
            }
        }

        if (systemPrompt is not null)
        {
            result.Insert(0, systemPrompt);
        }

        return result;
    }
}
