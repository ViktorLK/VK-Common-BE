using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Presence.Internal;

/// <summary>
/// Default token quota provider that fallbacks to system-wide configurations.
/// Follows AP.01, AP.03, and CS.06.
/// </summary>
internal sealed class DefaultPresenceQuotaProvider : IVKPresenceQuotaProvider
{
    private readonly VKPresenceOptions _options;

    public DefaultPresenceQuotaProvider(IOptions<VKPresenceOptions> options)
    {
        _options = VKGuard.NotNull(options).Value;
    }

    /// <inheritdoc />
    public Task<VKResult<VKPresenceQuota>> GetQuotaAsync(
        string tenantId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var quota = new VKPresenceQuota
        {
            TokenLimit = _options.DefaultTokenLimit,
            MaxRequestTokenQuota = _options.MaxRequestTokenQuota,
            SafetyMarginTokens = _options.SafetyMarginTokens
        };

        return Task.FromResult(VKResult.Success(quota));
    }
}
