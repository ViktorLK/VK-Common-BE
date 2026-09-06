using System.Collections.Generic;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Execution confidence and diagnostic governance metadata produced during materialization lifecycle.
/// Complies with [AP.01].
/// </summary>
public sealed record VKMaterializationConfidenceMetadata // [AP.01]
{
    /// <summary>
    /// The final effective expression mode used to obtain the valid response.
    /// </summary>
    public required VKAIEidosExpressionMode EffectiveMode { get; init; }

    /// <summary>
    /// Indicates whether the call was downgraded to a fallback mode.
    /// </summary>
    public bool IsDegraded { get; init; }

    /// <summary>
    /// The number of auto-repair retry attempts performed.
    /// </summary>
    public int RepairAttempts { get; init; }

    /// <summary>
    /// Estimated compliance score (0.0 to 1.0) based on validation errors.
    /// </summary>
    public double ComplianceScore { get; init; } = 1.0;

    /// <summary>
    /// The tolerance mode used for binding.
    /// </summary>
    public VKMaterializationToleranceMode ToleranceMode { get; init; } = VKMaterializationToleranceMode.Strict;

    /// <summary>
    /// All accumulated validation errors or warnings during the lifecycle.
    /// </summary>
    public IReadOnlyList<VKMaterializationValidationError> AccumulatedErrors { get; init; } = [];

    /// <summary>
    /// Total duration in milliseconds spent on negotiation, extraction, validation, and binding.
    /// </summary>
    public long DurationMs { get; init; }
}
