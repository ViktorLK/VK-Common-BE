using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Tokenics.Limiting.Internal;

/// <summary>
/// No-op implementation of <see cref="IVKTokenRateLimiter"/>.
/// Always succeeds without applying any actual limiting.
/// </summary>
internal sealed class NoOpVKTokenRateLimiter : IVKTokenRateLimiter
{
    private readonly IOptions<VKLimitingOptions> _options;

    public NoOpVKTokenRateLimiter(IOptions<VKLimitingOptions> options)
    {
        _options = VKGuard.NotNull(options);
    }

    /// <inheritdoc />
    public Task<VKResult> AcquireAsync(int estimatedTokens, CancellationToken cancellationToken = default)
    {
        _ = estimatedTokens;
        _ = cancellationToken;

        var options = _options.Value;
        if (!options.Enabled)
        {
            return Task.FromResult(VKResult.Success());
        }

        // Placeholder that always succeeds to allow execution without a real limiter
        return Task.FromResult(VKResult.Success());
    }

    /// <inheritdoc />
    public Task ReportUsageAsync(int actualTokens)
    {
        _ = actualTokens;
        return Task.CompletedTask;
    }
}
