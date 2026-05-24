using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    public VKResult<VKPromptTapestry> Weave(IReadOnlyList<VKFormattedTier> formatted, VKWeavingContext context)
    {
        VKGuard.NotNull(formatted);
        VKGuard.NotNull(context);

        var finalMessages = new List<VKChatMessage>();
        var systemInstructionsBuilder = new StringBuilder();

        // 1. Determine Tier Order based on Intent
        var intentMode = context.Intent;
        if (!_options.LayoutStrategies.TryGetValue(intentMode, out var tierOrder))
        {
            // Fallback to Chat order if missing
            if (!_options.LayoutStrategies.TryGetValue(VKIntent.Chat, out tierOrder))
            {
                tierOrder = new[] { VKPromptTierType.SystemInstructions, VKPromptTierType.Knowledge, VKPromptTierType.ChatHistory };
            }
        }

        // We will process tiers in the strict order defined by the template
        foreach (var tierType in tierOrder)
        {
            // Special handling for SystemInstructions which aggregates base pipeline system instructions
            if (tierType == VKPromptTierType.SystemInstructions)
            {
                var systemTiers = formatted.Where(f => f.TierType == VKPromptTierType.SystemInstructions).ToList();
                foreach (var tier in systemTiers)
                {
                    if (systemInstructionsBuilder.Length > 0)
                        systemInstructionsBuilder.AppendLine();
                    systemInstructionsBuilder.Append(tier.Content);
                }

                if (!string.IsNullOrWhiteSpace(context.Pipeline.SystemInstructions))
                {
                    if (systemInstructionsBuilder.Length > 0)
                        systemInstructionsBuilder.AppendLine();
                    systemInstructionsBuilder.Append(context.Pipeline.SystemInstructions);
                }
            }
            else if (tierType == VKPromptTierType.ChatHistory)
            {
                // History is strictly ordered by Depth (oldest first or newest last, typically descending depth = 3, 2, 1, 0)
                var historyTiers = formatted.Where(f => f.TierType == VKPromptTierType.ChatHistory)
                                            .OrderByDescending(f => f.Depth)
                                            .ToList();

                foreach (var tier in historyTiers)
                {
                    finalMessages.Add(new VKChatMessage { Role = tier.Role, Content = tier.Content });
                }
            }
            else
            {
                // Generalized appending for other tiers (Persona, Knowledge, Scenario, AuthorNote)
                var specializedTiers = formatted.Where(f => f.TierType == tierType).ToList();

                foreach (var tier in specializedTiers)
                {
                    // If it's defined as a system role internally, we can either append to system string or push as a system message
                    // In most modern APIs (like OpenAI), system messages can only be at the top. 
                    // However, we output ChatMessages, so we can preserve Role.
                    finalMessages.Add(new VKChatMessage { Role = tier.Role, Content = tier.Content });
                }
            }
        }

        // Combine the accumulated system instructions block
        string finalSystemText = systemInstructionsBuilder.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(finalSystemText))
        {
            // System instructions always go first in the final output array
            finalMessages.Insert(0, new VKChatMessage { Role = VKChatRole.System, Content = finalSystemText });
        }

        // Ensure Tapestry Output
        var tapestry = new VKPromptTapestry
        {
            Messages = finalMessages,
            SystemInstructions = finalSystemText,
            TotalEstimatedTokens = 0 // Resolved post-weave if needed
        };

        return VKResult.Success(tapestry);
    }
}
