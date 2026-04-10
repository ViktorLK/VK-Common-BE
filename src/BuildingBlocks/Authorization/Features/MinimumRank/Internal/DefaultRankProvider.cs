using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Authorization.DependencyInjection;

namespace VK.Blocks.Authorization.Features.MinimumRank.Internal;

/// <inheritdoc />
public sealed class DefaultRankProvider(IOptions<VKAuthorizationOptions> options) : IRankProvider
{
    private readonly VKAuthorizationOptions _options = options.Value;

    /// <inheritdoc />
    public ValueTask<string?> GetRankAsync(ClaimsPrincipal user, CancellationToken ct = default)
    {
        return ValueTask.FromResult(user.FindFirstValue(_options.RankClaimType));
    }
}
