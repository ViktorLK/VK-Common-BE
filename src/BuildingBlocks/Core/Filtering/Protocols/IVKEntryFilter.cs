using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Core;

/// <summary>
/// Defines a contract to filter a specific entry with a context.
/// </summary>
public interface IVKEntryFilter<in TEntry, in TContext>
    where TEntry : class
{
    /// <summary>
    /// Execution order of the filter. Lower values execute first.
    /// </summary>
    int FilterOrder { get; }

    /// <summary>
    /// Evaluates if the entry matches the filter criteria.
    /// </summary>
    Task<VKResult<VKFilterVerdict>> FilterAsync(
        TEntry entry,
        TContext context,
        CancellationToken ct = default);
}
