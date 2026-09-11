using VK.Blocks.Core;
using VK.Blocks.AI.Psyche.Common.Internal;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Configuration settings for the Knowledge feature.
/// </summary>
public sealed partial record VKKnowledgeOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Knowledge feature is enabled.
    /// Defaults to true.
    /// </summary>
    [VKRequestOverride]
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the number of historical dialogue echoes to scan for keyword matching.
    /// 0 matches only the current user input.
    /// N matches the current user input plus the last N echoes.
    /// -1 matches the entire dialogue history present in the context.
    /// Defaults to 5.
    /// </summary>
    [VKRequestOverride]
    public int KeywordScanDepth { get; init; } = 5;

    /// <summary>
    /// Gets or sets the maximum number of knowledge entries to inject into the prompt context.
    /// If null, no entry count limit is imposed. If set, must be greater than zero.
    /// Defaults to null (unconstrained by default).
    /// </summary>
    [VKRequestOverride]
    public int? MaxEntriesToInject { get; init; } = null;

    /// <summary>
    /// Gets or sets the number of tokens reserved for knowledge entries in the prompt context.
    /// If null, no local token ceiling is imposed, relying on global prompt weaving budget. If set, must be greater than zero.
    /// Defaults to null (unconstrained by default).
    /// </summary>
    [VKRequestOverride]
    public int? ReservedTokens { get; init; } = null;

    /// <summary>
    /// Gets or sets the default XML wrapper tag for knowledge segments when entry.Segment.TagName is omitted.
    /// Defaults to <see cref="PsycheConstants.XmlTags.Knowledge"/> ("knowledge").
    /// </summary>
    [VKRequestOverride]
    public string DefaultXmlTag { get; init; } = PsycheConstants.XmlTags.Knowledge;
}
