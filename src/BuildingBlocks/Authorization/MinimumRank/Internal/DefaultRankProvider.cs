using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;


namespace VK.Blocks.Authorization.MinimumRank.Internal;

/// <inheritdoc />
internal sealed class DefaultRankProvider(
    IOptions<VKMinimumRankOptions> options,
    IOptions<VKAuthorizationDefaultsOptions> globalOptions) : IVKRankProvider
{
    private readonly VKMinimumRankOptions _options = options.Value;
    private readonly VKAuthorizationDefaultsOptions _globalOptions = globalOptions.Value;

    /// <inheritdoc />
    public ValueTask<string?> GetRankAsync(ClaimsPrincipal user, CancellationToken ct = default)
    {
        var claimType = _options.RankClaimType ?? _globalOptions.RankClaimType;
        return ValueTask.FromResult(user.FindFirstValue(claimType));
    }
}
