using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Strategy for consolidating AI engrams.
/// </summary>
public interface IVKConsolidationStrategy
{
    /// <summary>
    /// Consolidates multiple items of content into a single output.
    /// </summary>
    Task<VKResult<string>> ConsolidateAsync(string[] contents, CancellationToken cancellationToken = default);

    /// <summary>
    /// Consolidates multiple memory entries into a single output.
    /// </summary>
    Task<VKResult<string>> ConsolidateMemoriesAsync(VKMemoryEntry[] memories, CancellationToken cancellationToken = default)
        => ConsolidateAsync(memories.Select(m => m.Content).ToArray(), cancellationToken);
}
