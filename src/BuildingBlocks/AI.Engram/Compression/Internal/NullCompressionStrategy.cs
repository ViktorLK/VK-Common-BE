using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Compression.Strategies;

/// <summary>
/// Default compression strategy that returns content unchanged.
/// </summary>
internal sealed class NullCompressionStrategy : IVKCompressionStrategy
{
    public Task<VKResult<string>> CompressAsync(string content, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(content);
        return Task.FromResult(VKResult.Success(content));
    }
}
