using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

// // [AP.03] Internal implementation inside Internal/ folder without VK prefix
namespace VK.Blocks.AI.Cognitive.Weaving.Internal;

internal sealed class BasicTapestryWeaver : IVKTapestryWeaver
{
    private readonly VKWeavingOptions _options;

    public BasicTapestryWeaver(IOptions<VKWeavingOptions>? options = null)
    {
        _options = options?.Value ?? new VKWeavingOptions();
    }

    public async Task<VKResult<VKPromptTapestry>> WeaveAsync(IReadOnlyList<VKFormattedTier> formatted, VKOrchestrationPipelineContext context)
    {
        VKGuard.NotNull(formatted);
        VKGuard.NotNull(context);

        var systemBuilder = new StringBuilder();
        void AppendSystem(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            if (systemBuilder.Length > 0) systemBuilder.AppendLine();
            systemBuilder.Append(text);
        }

        // --- Pass 1: Build Base Chat Timeline ---
        // Ordered by descending depth (oldest first, e.g., 3, 2, 1, 0)
        var historyTiers = formatted.Where(f => f.TierType == VKPromptTierType.ChatHistory)
                                    .OrderByDescending(f => f.Depth)
                                    .ToList();

        // Use a list of wrappers to allow insertion and modification of messages
        var timeline = historyTiers.Select(t => new TimelineEntry 
        { 
            Depth = t.Depth, 
            Message = new VKChatMessage { Role = t.Role, Content = t.Content } 
        }).ToList();

        // --- Pass 2: Positional Injection ---
        var nonHistory = formatted.Where(f => f.TierType != VKPromptTierType.ChatHistory).ToList();

        // 2a. Global System Block Assembly (XML & Markdown Layers)
        // Order: SystemInstructions -> BeforeDefs -> Persona -> AfterDefs -> BeforeExample -> AfterExample
        var processedTiers = new HashSet<VKFormattedTier>();

        // Core System Instructions
        foreach (var tier in nonHistory.Where(f => f.TierType == VKPromptTierType.SystemInstructions))
        {
            AppendSystem(tier.Content);
            processedTiers.Add(tier);
        }

        // Before Defs
        foreach (var tier in nonHistory.Where(f => !processedTiers.Contains(f) && f.Position == VKKnowledgePositions.BeforeDefs && f.TierType != VKPromptTierType.Persona))
        {
            AppendSystem(tier.Content);
            processedTiers.Add(tier);
        }

        // Persona (Forced to middle of system block if not explicitly placed elsewhere)
        foreach (var tier in nonHistory.Where(f => !processedTiers.Contains(f) && f.TierType == VKPromptTierType.Persona))
        {
            AppendSystem(tier.Content);
            processedTiers.Add(tier);
        }

        // Other Global System Placements
        var globalPositions = new[] { 
            VKKnowledgePositions.AfterDefs, 
            VKKnowledgePositions.BeforeExampleMessages, 
            VKKnowledgePositions.AfterExampleMessages 
        };

        foreach (var pos in globalPositions)
        {
            foreach (var tier in nonHistory.Where(f => !processedTiers.Contains(f) && f.Position == pos))
            {
                AppendSystem(tier.Content);
                processedTiers.Add(tier);
            }
        }

        // 2b. In-Chat Timeline Injection (Bracket & Meta Layers)
        foreach (var tier in nonHistory.Where(f => !processedTiers.Contains(f)))
        {
            int targetDepth = tier.Depth;

            // Handle special anchoring
            if (tier.Position == VKKnowledgePositions.BeforeAuthorNote || tier.Position == VKKnowledgePositions.AfterAuthorNote)
            {
                targetDepth = 0; // Pin to the very bottom
            }

            if (tier.Position == VKKnowledgePositions.SystemAtDepth)
            {
                // Inject as a pure System message directly into the timeline before the target depth
                var insertIndex = timeline.FindIndex(x => x.Depth == targetDepth);
                var newEntry = new TimelineEntry { Depth = targetDepth, Message = new VKChatMessage { Role = VKChatRole.System, Content = tier.Content } };
                
                if (insertIndex >= 0) timeline.Insert(insertIndex, newEntry);
                else timeline.Add(newEntry);
            }
            else
            {
                // In-Chat Bracket Injection: append directly to the target depth's message
                var targetEntry = timeline.FirstOrDefault(x => x.Depth == targetDepth);
                string bracketPrefix = tier.TierType == VKPromptTierType.Knowledge ? "System Knowledge" : tier.TierType.ToString();
                string bracketedNote = $"\n\n[{bracketPrefix}: {tier.Content}]";

                if (targetEntry != null)
                {
                    targetEntry.Message = targetEntry.Message with { Content = targetEntry.Message.Content + bracketedNote };
                }
                else
                {
                    // Fallback if the history doesn't go that deep or is empty
                    timeline.Add(new TimelineEntry { Depth = targetDepth, Message = new VKChatMessage { Role = VKChatRole.User, Content = bracketedNote.TrimStart() } });
                }
            }
        }

        // --- Pass 3: Final Assembly ---
        var finalMessages = new List<VKChatMessage>();
        
        string finalSystemText = systemBuilder.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(finalSystemText))
        {
            finalMessages.Add(new VKChatMessage { Role = VKChatRole.System, Content = finalSystemText });
        }

        // Timeline is already in chronological order (descending depth means 3 -> 2 -> 1 -> 0)
        // But we inserted SystemAtDepth before the element. We don't need to resort unless fallback additions broke the order.
        // Re-sorting by depth descending to be absolutely safe
        foreach (var entry in timeline.OrderByDescending(x => x.Depth))
        {
            finalMessages.Add(entry.Message);
        }

        var tapestry = new VKPromptTapestry
        {
            Messages = finalMessages,
            SystemInstructions = finalSystemText,
            TotalEstimatedTokens = 0 // Resolved post-weave if needed
        };

        return await Task.FromResult(VKResult.Success(tapestry));
    }

    private class TimelineEntry
    {
        public int Depth { get; set; }
        public required VKChatMessage Message { get; set; }
    }
}
