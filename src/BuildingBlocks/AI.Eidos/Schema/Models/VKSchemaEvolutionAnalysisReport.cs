using System.Collections.Generic;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Comprehensive offline analysis report evaluating the impact of schema evolution across historical payloads.
/// Complies with AP.01 (sealed record).
/// </summary>
public sealed record VKSchemaEvolutionAnalysisReport
{
    /// <summary>
    /// Static AST compatibility comparison between source and target schemas.
    /// </summary>
    public required VKSchemaCompatibilityReport Compatibility { get; init; }

    /// <summary>
    /// Total number of historical payloads evaluated.
    /// </summary>
    public required int TotalSamples { get; init; }

    /// <summary>
    /// Number of historical samples that successfully validated under the new schema.
    /// </summary>
    public required int PassedSamples { get; init; }

    /// <summary>
    /// Number of historical samples that failed validation under the new schema.
    /// </summary>
    public required int FailedSamples { get; init; }

    /// <summary>
    /// Pass rate percentage (0.0 to 1.0).
    /// </summary>
    public double PassRate => TotalSamples > 0 ? (double)PassedSamples / TotalSamples : 1.0;

    /// <summary>
    /// Detailed sample-by-sample analysis results.
    /// </summary>
    public IReadOnlyList<VKSchemaEvolutionSampleResult> SampleResults { get; init; } = [];
}
