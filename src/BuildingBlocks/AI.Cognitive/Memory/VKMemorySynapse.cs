namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Graph Edge: Represents a structural or logical connection between two memory traces.
/// <para>
/// Memories in advanced cognitive systems form a Knowledge Graph rather than a flat list.
/// A Synapse defines the relational edge (e.g., "Supports", "Contradicts", "DerivesFrom") 
/// between a Source Trace and a Target Trace, empowering the AI with logical reasoning capabilities.
/// </para>
/// </summary>
public sealed record VKMemorySynapse
{
    /// <summary>
    /// Gets the identifier of the originating memory trace.
    /// </summary>
    public required string SourceTraceId { get; init; }

    /// <summary>
    /// Gets the identifier of the destination memory trace.
    /// </summary>
    public required string TargetTraceId { get; init; }

    /// <summary>
    /// Gets the semantic relation defining how the Source relates to the Target 
    /// (e.g., "Contradicts", "EvolvesInto", "ProvidesContextFor").
    /// </summary>
    public required string RelationType { get; init; }

    /// <summary>
    /// Gets the synaptic weight (0.0 to 1.0) defining the strength of this logical connection.
    /// </summary>
    public float Weight { get; init; } = 1.0f;
}
