using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Physical capabilities and metadata for a specific AI model.
/// </summary>
public sealed record VKAIModelMetadata
{
    /// <summary>
    /// Gets the provider type hosting this model.
    /// </summary>
    public required VKAIProviderType Provider { get; init; }

    /// <summary>
    /// Gets the unique identifier or alias for the model.
    /// </summary>
    public required string ModelId { get; init; }

    /// <summary>
    /// Gets the total physical context window limit (prompt + completion tokens).
    /// </summary>
    public required int ContextWindowSize { get; init; }

    /// <summary>
    /// Gets the physical maximum number of output tokens the model can generate in a single response.
    /// </summary>
    public required int MaxOutputTokens { get; init; }

    /// <summary>
    /// Gets whether this metadata represents a known catalog entry rather than an unlisted fallback.
    /// </summary>
    public bool IsKnown { get; init; } = true;

    /// <summary>
    /// Gets the three-state streaming support for the model.
    /// </summary>
    public VKModelCapabilityState StreamingCapability { get; init; } = VKModelCapabilityState.Unknown;

    /// <summary>
    /// Gets the three-state structured JSON schema output support for the model.
    /// </summary>
    public VKModelCapabilityState StructuredOutputCapability { get; init; } = VKModelCapabilityState.Unknown;

    /// <summary>
    /// Gets the three-state tool / function calling support for the model.
    /// </summary>
    public VKModelCapabilityState ToolsCapability { get; init; } = VKModelCapabilityState.Unknown;

    /// <summary>
    /// Gets the three-state reasoning / thinking mode support for the model.
    /// </summary>
    public VKModelCapabilityState ReasoningCapability { get; init; } = VKModelCapabilityState.Unknown;

    /// <summary>
    /// Gets whether the model supports streaming responses.
    /// </summary>
    public bool SupportsStreaming
    {
        get => StreamingCapability == VKModelCapabilityState.Supported;
        init => StreamingCapability = value ? VKModelCapabilityState.Supported : VKModelCapabilityState.Unsupported;
    }

    /// <summary>
    /// Gets whether the model supports native structured JSON schema output.
    /// </summary>
    public bool SupportsStructuredOutput
    {
        get => StructuredOutputCapability == VKModelCapabilityState.Supported;
        init => StructuredOutputCapability = value ? VKModelCapabilityState.Supported : VKModelCapabilityState.Unsupported;
    }

    /// <summary>
    /// Gets whether the model supports tool / function calling.
    /// </summary>
    public bool SupportsTools
    {
        get => ToolsCapability == VKModelCapabilityState.Supported;
        init => ToolsCapability = value ? VKModelCapabilityState.Supported : VKModelCapabilityState.Unsupported;
    }

    /// <summary>
    /// Gets whether the model supports reasoning / thinking mode.
    /// </summary>
    public bool SupportsReasoning
    {
        get => ReasoningCapability == VKModelCapabilityState.Supported;
        init => ReasoningCapability = value ? VKModelCapabilityState.Supported : VKModelCapabilityState.Unsupported;
    }

    /// <summary>
    /// Gets the supported reasoning effort levels (e.g., "low", "medium", "high"), if applicable.
    /// </summary>
    public IReadOnlyList<string>? ReasoningEffortLevels { get; init; }

    /// <summary>
    /// Gets whether the model is deprecated or scheduled for retirement.
    /// </summary>
    public bool IsDeprecated { get; init; } = false;
}
