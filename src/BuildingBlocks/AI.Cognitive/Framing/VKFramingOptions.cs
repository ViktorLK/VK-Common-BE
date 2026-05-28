using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Configuration options for the Cognitive Framing feature.
/// Follows AP.01, AP.03, and BB.07.
/// </summary>
[VKFeature(typeof(VKAICognitiveBlock), GenerateArgs = true, GenerateValidator = true, Namespace = "VK.Blocks.AI.Cognitive.Framing")]
public sealed partial record VKFramingOptions : IVKFramingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Cognitive Framing is enabled.
    /// Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the default token limit for sliding window truncation.
    /// Defaults to 32,768 tokens.
    /// </summary>
    public int DefaultTokenLimit { get; init; } = 32768;

    /// <summary>
    /// Gets or sets the warning threshold ratio (0.0 to 1.0) before triggering warnings.
    /// Defaults to 0.8 (80% of capacity).
    /// </summary>
    public float TruncationThreshold { get; init; } = 0.8f;

    /// <summary>
    /// Gets or sets the maximum request-level token quota limit.
    /// Defaults to 8192 tokens.
    /// </summary>
    public int MaxRequestTokenQuota { get; init; } = 8192;

    /// <summary>
    /// Gets or sets the safety token margin buffer before triggering absolute window clipping.
    /// Defaults to 512 tokens.
    /// </summary>
    public int SafetyMarginTokens { get; init; } = 512;

    /// <summary>
    /// Gets or sets the environmental label (e.g. "Development", "Staging", "Production").
    /// Defaults to "Production".
    /// </summary>
    public string Environment { get; init; } = "Production";
}
