using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Strategy for pruning AI engrams.
/// </summary>
public interface IVKPruningStrategy
{
    /// <summary>
    /// Evaluates whether the content should be pruned based on its score.
    /// </summary>
    Task<VKResult<bool>> ShouldPruneAsync(string content, double score, CancellationToken cancellationToken = default);
}
