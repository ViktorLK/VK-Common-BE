namespace VK.Blocks.AI.Engram;

/// <summary>
/// Options for enrichment features during compression.
/// </summary>
public sealed partial record VKCompressionEnrichmentOptions
{
    public bool Timeline { get; init; } = false;
    public bool Contradictions { get; init; } = false;
    public bool ActionItems { get; init; } = false;
    public bool Confidence { get; init; } = false;
    public bool PredictiveCue { get; init; } = false;
    public bool EmotionalTagging { get; init; } = false;
    public bool SalienceWeighting { get; init; } = true;
}
