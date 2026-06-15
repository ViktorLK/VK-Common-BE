using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Core;

/// <summary>
/// Provides extension methods for executing filter chains.
/// </summary>
public static class VKFilterExtensions
{
    /// <summary>
    /// Applies a chain of filters sequentially on a given entry and context.
    /// Filters are automatically sorted by their FilterOrder property before execution.
    /// Handles short-circuiting on ForceKeep and Reject.
    /// </summary>
    public static async Task<VKResult<VKFilterVerdict>> ApplyFiltersAsync<TEntry, TContext>(
        this IEnumerable<IVKEntryFilter<TEntry, TContext>> filters,
        TEntry entry,
        TContext context,
        CancellationToken ct = default)
        where TEntry : class
    {
        VKGuard.NotNull(filters);
        VKGuard.NotNull(entry);

        IOrderedEnumerable<IVKEntryFilter<TEntry, TContext>> sortedFilters = filters.OrderBy(f => f.FilterOrder);

        foreach (IVKEntryFilter<TEntry, TContext> filter in sortedFilters)
        {
            VKResult<VKFilterVerdict> result = await filter.FilterAsync(entry, context, ct).ConfigureAwait(false); // [CS.03]
            if (!result.IsSuccess)
            {
                return VKResult.Failure<VKFilterVerdict>(result.FirstError);
            }

            if (result.Value == VKFilterVerdict.ForceKeep)
            {
                return VKResult.Success(VKFilterVerdict.ForceKeep);
            }

            if (result.Value == VKFilterVerdict.Reject)
            {
                return VKResult.Success(VKFilterVerdict.Reject);
            }
        }

        return VKResult.Success(VKFilterVerdict.Keep);
    }
}
