using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Filtering.Internal;

/// <summary>
/// Filter that gates entry injection based on the approval workflow status.
/// Rejects any entry that is not in <see cref="VKKnowledgeApprovalStatus.Approved"/> state.
/// Follows CS.01, CS.03, AP.01.
/// </summary>
internal sealed class ApprovalStatusFilter : IVKKnowledgeLifecycleFilter
{
    /// <inheritdoc />
    public int FilterOrder => 15;

    /// <inheritdoc />
    public Task<VKResult<VKFilterVerdict>> FilterAsync(
        VKKnowledgeLifecycleEntry entry,
        VKCorpusContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entry);
        VKGuard.NotNull(context);

        if (entry.Lifecycle.ApprovalStatus != VKKnowledgeApprovalStatus.Approved)
        {
            return Task.FromResult(VKResult.Success(VKFilterVerdict.Reject));
        }

        return Task.FromResult(VKResult.Success(VKFilterVerdict.Keep));
    }
}
