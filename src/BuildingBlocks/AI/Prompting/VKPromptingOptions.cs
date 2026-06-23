using System.Reflection;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Prompting feature.
/// </summary>
[VKFeature(typeof(VKAIBlock))]
public sealed partial record VKPromptingOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Prompting feature is enabled.
    /// Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the base directory for loading file-based prompts.
    /// If null, file-based loading is disabled.
    /// </summary>
    public string? BaseDirectory { get; init; }

    /// <summary>
    /// Gets or sets the assembly to load embedded prompts from.
    /// If null, the executing assembly is used.
    /// </summary>
    public Assembly? EmbeddedPromptAssembly { get; init; }

    /// <summary>
    /// Gets or sets the base namespace for embedded prompts.
    /// If null, embedded loading is disabled.
    /// </summary>
    public string? EmbeddedPromptNamespace { get; init; }
}
