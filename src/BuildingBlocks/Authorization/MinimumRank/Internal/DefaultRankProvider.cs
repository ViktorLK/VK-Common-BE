using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;


namespace VK.Blocks.Authorization.MinimumRank.Internal;

/// <inheritdoc />
internal sealed class DefaultRankProvider(IOptions<VKMinimumRankOptions> options) : IVKRankProvider
{
    private readonly VKMinimumRankOptions _options = options.Value;

    /// <inheritdoc />
    public ValueTask<string?> GetRankAsync(ClaimsPrincipal user, CancellationToken ct = default)
    {
        return ValueTask.FromResult(user.FindFirstValue(_options.RankClaimType));
    }
}
