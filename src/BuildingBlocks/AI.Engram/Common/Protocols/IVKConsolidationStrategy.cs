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
}
