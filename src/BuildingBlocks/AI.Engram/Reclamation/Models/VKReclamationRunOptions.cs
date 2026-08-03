namespace VK.Blocks.AI.Engram;

/// <summary>
/// Execution options for a specific memory reclamation run.
/// </summary>
public sealed record VKReclamationRunOptions
{
    /// <summary>
    /// Gets a value indicating whether the reclamation cycle should run in DryRun mode.
    /// In DryRun mode, decay and pruning candidate evaluations are computed and logged in the result,
    /// but no physical deletions, archives, or vector store purges are executed.
    /// </summary>
    public bool DryRun { get; init; }

    /// <summary>
    /// Gets an optional batch size override for this execution run.
    /// </summary>
    public int? BatchSizeOverride { get; init; }
}
