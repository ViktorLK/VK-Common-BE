using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Cognitive;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Presence.Internal;

/// <summary>
/// Default token quota provider that fallbacks to system-wide configurations.
/// Follows AP.01, AP.03, and CS.06.
/// </summary>
internal sealed class DefaultPresenceQuotaProvider : IVKPresenceQuotaProvider
{
    private readonly VKFramingOptions _framingOptions;

    public DefaultPresenceQuotaProvider(IOptions<VKFramingOptions> framingOptions)
    {
        _framingOptions = VKGuard.NotNull(framingOptions).Value;
    }

    /// <inheritdoc />
    public Task<VKResult<VKPresenceQuota>> GetQuotaAsync(
        string tenantId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var quota = new VKPresenceQuota
        {
            TokenLimit = _framingOptions.DefaultTokenLimit,
            MaxRequestTokenQuota = _framingOptions.MaxRequestTokenQuota,
            SafetyMarginTokens = _framingOptions.SafetyMarginTokens
        };

        return Task.FromResult(VKResult.Success(quota));
    }
}
