using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Strategy for scoring AI engrams.
/// </summary>
public interface IVKScoringStrategy
{
    /// <summary>
    /// Calculates a score for the input content.
    /// </summary>
    Task<VKResult<double>> ScoreAsync(string content, CancellationToken cancellationToken = default);
}
