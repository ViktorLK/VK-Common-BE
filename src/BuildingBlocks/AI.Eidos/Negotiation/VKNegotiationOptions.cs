using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Options for Eidos Negotiation Feature slice.
/// </summary>
public sealed partial record VKNegotiationOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the default preferred expression mode when capabilities support native structured output.
    /// Defaults to StructuredOutput.
    /// </summary>
    public VKAIEidosExpressionMode DefaultPreferredMode { get; init; } = VKAIEidosExpressionMode.StructuredOutput;

    /// <summary>
    /// Custom capability overrides mapped by "provider:model" or "provider" keys.
    /// </summary>
    public IReadOnlyDictionary<string, VKAIEidosProviderCapabilities> ProviderCapabilitiesOverrides { get; init; } = new Dictionary<string, VKAIEidosProviderCapabilities>();

    /// <summary>
    /// Whether to prevent falling back to PromptJson when the schema is classified as complex.
    /// </summary>
    public bool DisallowPromptJsonForComplexSchemas { get; init; } = true;

    /// <summary>
    /// Maximum number of required properties before a schema is marked as complex.
    /// </summary>
    public int ComplexSchemaPropertyThreshold { get; init; } = 5;

    /// <summary>
    /// Whether to automatically synthesize and inject a minimal valid JSON example in PromptJson instructions.
    /// </summary>
    public bool InjectPromptFewShotExample { get; init; } = true;
}
