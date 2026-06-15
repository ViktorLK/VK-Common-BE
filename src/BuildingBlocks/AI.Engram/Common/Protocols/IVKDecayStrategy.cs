using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Strategy for applying decay to AI engrams.
/// </summary>
public interface IVKDecayStrategy
{
    /// <summary>
    /// Applies decay logic to the input content.
    /// </summary>
    Task<VKResult<string>> DecayAsync(string content, CancellationToken cancellationToken = default);
}
