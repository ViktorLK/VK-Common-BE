using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the public contract interface for Cognitive Framing configuration options.
/// Follows AP.01, AP.03.
/// </summary>
public interface IVKFramingOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets the default token limit for sliding window truncation.
    /// </summary>
    int DefaultTokenLimit { get; }

    /// <summary>
    /// Gets the warning threshold ratio before triggering compression or warnings.
    /// </summary>
    float TruncationThreshold { get; }

    /// <summary>
    /// Gets the maximum request-level token quota limit.
    /// </summary>
    int MaxRequestTokenQuota { get; }

    /// <summary>
    /// Gets the safety token margin buffer before triggering absolute window clipping.
    /// </summary>
    int SafetyMarginTokens { get; }

    /// <summary>
    /// Gets the environmental label (e.g. "Development", "Staging", "Production").
    /// </summary>
    string Environment { get; }
}
