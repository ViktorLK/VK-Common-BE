using System.Collections.Generic;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Individual sample validation outcome within an evolution analysis run.
/// Complies with AP.01 (sealed record).
/// </summary>
public sealed record VKSchemaEvolutionSampleResult
{
    /// <summary>
    /// Index position of the sample payload within the input batch.
    /// </summary>
    public required int SampleIndex { get; init; }

    /// <summary>
    /// True if the sample passed validation under the target schema.
    /// </summary>
    public required bool IsValid { get; init; }

    /// <summary>
    /// Detailed validation error taxonomy items, if any.
    /// </summary>
    public IReadOnlyList<VKExtractionValidationError> Errors { get; init; } = [];
}
