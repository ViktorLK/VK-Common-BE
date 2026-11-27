using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Ledger contract for idempotent AI cost recording and scope/tag aggregation.
/// </summary>
public interface IVKAICostLedger
{
    /// <summary>
    /// Records an invocation cost entry idempotently by <see cref="VKAICostLedgerEntry.InvocationId"/>.
    /// Returns true if newly recorded; false if an entry with the same InvocationId already exists.
    /// </summary>
    ValueTask<bool> RecordAsync(VKAICostLedgerEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Aggregates total priced cost for the specified scope and optional correlation tags.
    /// </summary>
    ValueTask<decimal> GetTotalCostAsync(
        VKAIScopeKey scopeKey,
        VKAICorrelationTags? tags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves ledger entries matching the specified scope and optional correlation tags.
    /// </summary>
    ValueTask<IReadOnlyList<VKAICostLedgerEntry>> GetEntriesAsync(
        VKAIScopeKey scopeKey,
        VKAICorrelationTags? tags = null,
        CancellationToken cancellationToken = default);
}
