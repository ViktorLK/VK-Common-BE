namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the categories of AI memory, each having different decay and pruning strategies.
/// </summary>
public enum VKMemoryCategory
{
    /// <summary>
    /// Sensory or biometric data. Decays very fast (e.g., deleted after 10 minutes).
    /// </summary>
    Biometrics = 0,

    /// <summary>
    /// Short-term/working memory. Candidates for summarization and distillation.
    /// </summary>
    ShortTerm = 1,

    /// <summary>
    /// Long-term facts and knowledge. Candidates for cold storage (Reality Ledger).
    /// </summary>
    LongTerm = 2,

    /// <summary>
    /// Core persona fragments. Immutable and protected from decay.
    /// </summary>
    Persona = 3
}
