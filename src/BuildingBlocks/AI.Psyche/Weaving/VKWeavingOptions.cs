using VK.Blocks.Core;
using VK.Blocks.AI.Psyche.Common.Internal;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Controls the global behavior and strict layout constraints of the Weaving Engine.
/// Follows BB.05 (Options pattern with sealed record).
/// Uses <see cref="VKArgsGenerationMode.Implicit"/> so all options are automatically available for request-level override.
/// </summary>
public sealed partial record VKWeavingOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the optional maximum context token budget.
    /// If null (default), dynamically uses the physical ContextWindowSize from <see cref="IVKVKAIModelCatalog"/>.
    /// </summary>
    public int? MaxContextBudget { get; init; } = null;

    /// <summary>
    /// Gets the reserved token budget allocated for LLM response generation when not specified in args.
    /// Default is 2,048.
    /// </summary>
    public int ResponseReservedTokens { get; init; } = 2048;

    /// <summary>
    /// Gets the default separator between segments and coalesced prompt items.
    /// Default is <see cref="Common.Internal.PsycheConstants.Separators.SegmentSeparator"/> ("\n\n").
    /// </summary>
    public string SegmentSeparator { get; init; } = PsycheConstants.Separators.SegmentSeparator;

    /// <summary>
    /// Gets whether to sanitize template variables (escaping XML tags and stripping ChatML control tokens)
    /// to prevent prompt injection. Default is true.
    /// </summary>
    public bool SanitizeVariables { get; init; } = true;

    /// <summary>
    /// Gets the maximum character length permitted for any single placeholder replacement value.
    /// If exceeded, the replacement task returns a failure to prevent memory exhaustion or DoS attacks.
    /// Default is 32,768 characters. Set to null to disable length enforcement.
    /// </summary>
    public int? MaxReplacementLength { get; init; } = 32_768;
}
