using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Options for the Search Guard middleware feature.
/// </summary>
[VKFeature(typeof(VKVectorSearchBlock))]
public sealed partial record VKSearchGuardOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Search Guard is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the minimum allowed query length.
    /// </summary>
    public int MinLength { get; init; } = 1;

    /// <summary>
    /// Gets or sets the maximum allowed query length.
    /// </summary>
    public int MaxLength { get; init; } = 1000;

    /// <summary>
    /// Gets or sets a value indicating whether SQL Injection protection is enabled.
    /// </summary>
    public bool EnableSqlInjectionProtection { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether Prompt Injection protection is enabled.
    /// </summary>
    public bool EnablePromptInjectionProtection { get; init; } = true;

    /// <summary>
    /// Gets or sets the list of blocked phrases.
    /// </summary>
    public List<string> BlockedPhrases { get; init; } = [];
}
