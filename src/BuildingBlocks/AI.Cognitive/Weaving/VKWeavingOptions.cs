using System;
using System.Collections.Generic;
using VK.Blocks.Core;

// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Controls the global behavior and strict layout constraints of the Weaving Engine.
/// Follows BB.05 (Options pattern with sealed record).
/// </summary>
public sealed record VKWeavingOptions : IVKWeavingOptions
{
    public static string SectionName => "VKBlocks:AI:Cognitive:Weaving";

    public int MaxTokenLimit { get; init; } = 8192;
    
    public bool StripThinkTags { get; init; } = true;
    public bool EnableSemanticPruning { get; init; } = true;

    /// <summary>
    /// Factory for resolving the default token meter if none is provided via the context budget.
    /// </summary>
    public Func<IServiceProvider, IVKTokenMeter>? DefaultTokenMeterFactory { get; init; }

    /// <summary>
    /// Defines the tier rendering order mapped to the active Intent.
    /// </summary>
    public IReadOnlyDictionary<VKIntent, IReadOnlyList<VKPromptTierType>> LayoutStrategies { get; init; } 
        = new Dictionary<VKIntent, IReadOnlyList<VKPromptTierType>>
        {
            { VKIntent.Chat, new[] { VKPromptTierType.SystemInstructions, VKPromptTierType.Knowledge, VKPromptTierType.ChatHistory } },
            { VKIntent.Roleplay, new[] { VKPromptTierType.Persona, VKPromptTierType.Scenario, VKPromptTierType.Knowledge, VKPromptTierType.ChatHistory, VKPromptTierType.AuthorNote } },
            { VKIntent.Consulting, new[] { VKPromptTierType.SystemInstructions, VKPromptTierType.Persona, VKPromptTierType.Knowledge, VKPromptTierType.ChatHistory, VKPromptTierType.AuthorNote } }
        };
}
