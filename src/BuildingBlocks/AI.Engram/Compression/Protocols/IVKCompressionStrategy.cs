using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Engram.Compression.Models;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Strategy for compressing AI engrams.
/// </summary>
public interface IVKCompressionStrategy
{
    /// <summary>
    /// Compresses the input content given a compression context.
    /// </summary>
    Task<VKResult<string>> CompressAsync(VKCompressionContext context, CancellationToken cancellationToken = default);
}
