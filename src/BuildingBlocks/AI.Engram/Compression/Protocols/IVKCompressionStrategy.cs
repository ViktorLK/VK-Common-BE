using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Strategy for compressing AI engrams.
/// </summary>
public interface IVKCompressionStrategy
{
    /// <summary>
    /// Compresses the input content.
    /// </summary>
    Task<VKResult<string>> CompressAsync(string content, CancellationToken cancellationToken = default);
}
