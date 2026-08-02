using System;
using VK.Blocks.AI;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Ingesting.Internal;

/// <summary>
/// Maps AI.Cognitive reflection proposals (<see cref="VKKnowledgeProposal"/>) into <see cref="VKKnowledgeLifecycleEntry"/>
/// with Flash filtering, Confidence inspection gates, and Double-Axis lifecycle controls.
/// Follows AP.01 and BB.01.
/// </summary>
internal static class KnowledgeProposalMapper
{
    /// <summary>
    /// Maps a raw reflection proposal into a Corpus knowledge lifecycle entry.
    /// Returns Failure if the proposal is Flash-scoped (bypassing VectorStore embedding).
    /// </summary>
    public static VKResult<VKKnowledgeLifecycleEntry> MapToCorpusEntry(
        VKKnowledgeProposal proposal,
        string? userId = null,
        string? tenantId = null)
    {
        VKGuard.NotNull(proposal);

        // 1. Intercept Flash-scoped knowledge: Bypass VectorStore & Embedding to save computation/storage
        if (string.Equals(proposal.RetentionPolicy, "Flash", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(proposal.InjectionFrequency, "Flash", StringComparison.OrdinalIgnoreCase))
        {
            return VKResult.Failure<VKKnowledgeLifecycleEntry>(
                new VKError("Corpus.Ingest.FlashBypassed", "Flash-scoped proposal is transient and bypassed from VectorStore ingestion."));
        }

        // 2. Confidence inspection gate (<0.5 rejected, 0.5 ~ 0.85 requires pending_review, >=0.85 auto-approved)
        if (proposal.Confidence < 0.5)
        {
            return VKResult.Failure<VKKnowledgeLifecycleEntry>(
                new VKError("Corpus.Ingest.LowConfidence", $"Proposal confidence {proposal.Confidence:F2} is below minimum threshold 0.50."));
        }

        bool isPendingReview = proposal.Confidence < 0.85;

        // 3. Axis A: Real-time Injection Frequency Control (Corpus Sticky & Cooldown)
        int? stickyTurns = proposal.InjectionFrequency?.ToUpperInvariant() switch
        {
            "PERSISTENTACTIVE" => VKKnowledgeLifecyclePresets.Sticky.Anchor, // -1 (Never expire)
            "TOPICCONTEXT" => VKKnowledgeLifecyclePresets.Sticky.Topic,       // 5 turns
            "PASSIVERECALL" => 0,                                             // 0 (Only retrieved via vector search, not sticky)
            _ => VKKnowledgeLifecyclePresets.Sticky.Topic
        };

        int? knowledgeCooldownTurns = proposal.InjectionFrequency?.ToUpperInvariant() switch
        {
            "PASSIVERECALL" => 9999, // Powerful cooldown: never proactively injected unless explicitly matched by vector search
            "TOPICCONTEXT" => VKKnowledgeLifecyclePresets.Cooldown.Short, // 3 turns cooldown to prevent nag
            _ => VKKnowledgeLifecyclePresets.Cooldown.None
        };

        // 4. Axis B: Long-term Storage Retention Policy (Engram Base Retention Score)
        double baseRetentionScore = proposal.RetentionPolicy?.ToUpperInvariant() switch
        {
            "PERMANENT" => 1.0,
            "LONGTERM" => 0.7,
            "TRANSIENT" => 0.3,
            _ => 0.5
        };

        // 5. Sensitivity User Isolation
        bool isPrivateOrSecret = string.Equals(proposal.Sensitivity, "Private", StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(proposal.Sensitivity, "Secret", StringComparison.OrdinalIgnoreCase);

        var entry = new VKKnowledgeLifecycleEntry
        {
            Knowledge = new VKKnowledgeEntry
            {
                Id = new VKKnowledgeId(Guid.NewGuid()),
                TriggerType = VKKnowledgeTriggerType.Constant,
                Segment = new VKPromptSegment
                {
                    Content = proposal.Content,
                    Name = $"auto-refinement-{Guid.NewGuid():N}",
                    IsEnabled = !isPendingReview // Disable active injection until approved if pending review
                }
            },
            Lifecycle = new VKKnowledgeLifecycle
            {
                Probability = proposal.Confidence,
                StickyTurns = stickyTurns,
                KnowledgeCooldownTurns = knowledgeCooldownTurns,
                CategoryTag = proposal.Category,
                TargetUserId = isPrivateOrSecret ? userId : null,
                IsPendingReview = isPendingReview,
                BaseRetentionScore = baseRetentionScore,
                Provenance = VKKnowledgeProvenance.AIExtracted,
                ApprovalStatus = isPendingReview ? VKKnowledgeApprovalStatus.PendingReview : VKKnowledgeApprovalStatus.Approved
            }
        };

        return VKResult.Success(entry);
    }
}
