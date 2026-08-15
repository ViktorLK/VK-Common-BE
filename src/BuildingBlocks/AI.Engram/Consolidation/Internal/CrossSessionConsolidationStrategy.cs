using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Consolidation.Internal;

/// <summary>
/// Cross-session consolidation strategy using LLM to discover patterns across multiple sessions.
/// </summary>
internal sealed class CrossSessionConsolidationStrategy : IVKCrossSessionConsolidationStrategy
{
    private readonly IVKChatEngine _chatEngine;
    private readonly VKConsolidationOptions _options;

    public CrossSessionConsolidationStrategy(
        IVKChatEngine chatEngine,
        IOptions<VKConsolidationOptions> options)
    {
        _chatEngine = VKGuard.NotNull(chatEngine);
        _options = VKGuard.NotNull(options?.Value);
    }

    public async Task<VKResult<IReadOnlyList<VKMemoryEntry>>> ConsolidateCrossSessionAsync(
        IReadOnlyList<VKMemoryEntry> sampledL3Memories,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(sampledL3Memories);

        if (sampledL3Memories.Count == 0)
        {
            return VKResult.Success<IReadOnlyList<VKMemoryEntry>>([]);
        }

        var memoryTexts = string.Join("\n", sampledL3Memories.Select((m, i) => $"[{i + 1}] Session:{m.SessionId} => {m.Content}"));
        string prompt = "You are a long-term cross-session memory consolidation manager.\n" +
                        "Analyze the following memory fragments collected across multiple user sessions.\n" +
                        "Identify overarching user preferences, recurring habits, or consolidated facts that span across sessions.\n" +
                        "Output the consolidated facts as a bulleted list (one per line starting with '- ').\n\n" +
                        $"MEMORIES:\n{memoryTexts}";

        var messages = new[] { VKChatMessage.FromText(VKChatRole.User, prompt) };
        VKChatArgs? chatArgs = null;
        string? targetModel = _options.FactExtractionModelId ?? _options.ModelId;
        if (!string.IsNullOrWhiteSpace(targetModel))
        {
            chatArgs = new VKChatArgs { ModelId = targetModel };
        }

        try
        {
            var result = await _chatEngine.SendAsync(messages, chatArgs, cancellationToken).ConfigureAwait(false);
            if (result.IsFailure)
            {
                return VKResult.Failure<IReadOnlyList<VKMemoryEntry>>(result.Errors);
            }

            string rawText = result.Value.Message.Content ?? string.Empty;
            var lines = rawText.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                               .Where(l => l.StartsWith('-') || l.StartsWith('*'))
                               .Select(l => l.TrimStart('-', '*', ' '))
                               .Where(l => !string.IsNullOrWhiteSpace(l))
                               .ToList();

            var newEntries = new List<VKMemoryEntry>();
            foreach (var fact in lines)
            {
                newEntries.Add(new VKMemoryEntry
                {
                    Id = new VKMemoryId(Guid.NewGuid()),
                    Content = fact,
                    Category = VKMemoryCategory.LongTerm,
                    Importance = 0.85f,
                    CreatedAt = DateTimeOffset.UtcNow,
                    Metadata = new Dictionary<string, string>
                    {
                        ["Source"] = "CrossSessionConsolidation",
                        ["ConsolidatedAt"] = DateTimeOffset.UtcNow.ToString("O")
                    }
                });
            }

            return VKResult.Success<IReadOnlyList<VKMemoryEntry>>(newEntries);
        }
        catch (Exception ex)
        {
            return VKResult.Failure<IReadOnlyList<VKMemoryEntry>>(new VKError("Engram.Consolidation.CrossSessionError", ex.Message));
        }
    }
}
