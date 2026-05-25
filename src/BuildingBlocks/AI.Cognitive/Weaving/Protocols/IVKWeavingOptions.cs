using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Aggregates all static Weaving configuration options.
/// </summary>
public interface IVKWeavingOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the maximum token limit for the weaving process.
    /// </summary>
    int MaxTokenLimit { get; }

    /// <summary>
    /// Gets whether to strip think tags (e.g. &lt;think&gt;) from the prompt.
    /// </summary>
    bool StripThinkTags { get; }

    /// <summary>
    /// Gets whether to enable semantic pruning when token limits are exceeded.
    /// </summary>
    bool EnableSemanticPruning { get; }

    /// <summary>
    /// Factory for resolving the default token meter if none is provided via the context budget.
    /// </summary>
    Func<IServiceProvider, IVKTokenMeter>? DefaultTokenMeterFactory { get; }

    /// <summary>
    /// Defines the tier rendering order mapped to the active Intent.
    /// </summary>
    IReadOnlyDictionary<VKIntent, IReadOnlyList<VKPromptTierType>> LayoutStrategies { get; }
}
