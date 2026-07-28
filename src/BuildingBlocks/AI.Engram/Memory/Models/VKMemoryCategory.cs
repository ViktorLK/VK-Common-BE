namespace VK.Blocks.AI.Engram;

/// <summary>
/// Defines the core memory tier categories in the engram lifecycle.
/// </summary>
public enum VKMemoryCategory
{
    /// <summary>
    /// L1 Short-term/working dialogue memory. Candidates for summarization and distillation.
    /// </summary>
    ShortTerm = 1,

    /// <summary>
    /// L2 Medium-term condensed summaries. Bridge between L1 short-term and L3 long-term consolidation.
    /// </summary>
    MediumTerm = 2,

    /// <summary>
    /// L3 Long-term facts, consolidated insights, and reality ledger records.
    /// </summary>
    LongTerm = 3
}
