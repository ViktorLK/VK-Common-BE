using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Engram.Compression.Models;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Compression.Internal;

/// <summary>
/// Default compression strategy that returns content unchanged.
/// </summary>
internal sealed class NullCompressionStrategy : IVKCompressionStrategy
{
    public Task<VKResult<string>> CompressAsync(VKCompressionContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);
        return Task.FromResult(VKResult.Success(context.Content));
    }
}
