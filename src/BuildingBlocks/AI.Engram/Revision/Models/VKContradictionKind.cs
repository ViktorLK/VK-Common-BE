namespace VK.Blocks.AI.Engram;

/// <summary>
/// Result type of a contradiction arbitration assessment.
/// </summary>
public enum VKContradictionKind
{
    /// <summary>
    /// No contradiction detected.
    /// </summary>
    None = 0,

    /// <summary>
    /// Explicit correction: new fact directly invalidates the old memory.
    /// </summary>
    ExplicitCorrection = 1,

    /// <summary>
    /// Semantic evolution: detail evolved over time without direct invalidation.
    /// </summary>
    SemanticDrift = 2,

    /// <summary>
    /// Unresolved contradiction: logical conflict exists without definitive resolution.
    /// </summary>
    UnresolvedContradiction = 3,

    /// <summary>
    /// Idempotent request: modification has already been applied or content is identical.
    /// </summary>
    NoOpIdempotent = 4
}
