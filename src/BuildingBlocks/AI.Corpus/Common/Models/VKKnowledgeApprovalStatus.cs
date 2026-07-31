namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Specifies the approval workflow status for a knowledge entry in the corpus.
/// </summary>
public enum VKKnowledgeApprovalStatus
{
    /// <summary>
    /// Approved and active for retrieval and injection.
    /// </summary>
    Approved = 0,

    /// <summary>
    /// Draft status (e.g. automatically extracted by AI), waiting for review.
    /// </summary>
    Draft = 1,

    /// <summary>
    /// Currently under review by human auditor or verification rule.
    /// </summary>
    PendingReview = 2,

    /// <summary>
    /// Rejected or archived, blocked from retrieval and injection.
    /// </summary>
    Rejected = 3
}
