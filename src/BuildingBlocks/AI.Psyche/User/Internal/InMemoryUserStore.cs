using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.User.Internal;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IVKUserStore"/> for local development and testing.
/// Follows AP.01 and CS.03.
/// </summary>
internal sealed class InMemoryUserStore : IVKUserStore
{
    private readonly ConcurrentDictionary<VKUserId, VKUserPresence> _presences = new();
    private readonly IVKIdentityContext _identityContext;

    public InMemoryUserStore(IVKIdentityContext identityContext)
    {
        _identityContext = VKGuard.NotNull(identityContext);
    }

    public Task<VKResult<VKUserPresence?>> GetPresenceAsync(
        VKUserId userId,
        CancellationToken cancellationToken = default)
    {
        if (!_presences.TryGetValue(userId, out var presence))
        {
            return Task.FromResult(VKResult.Success<VKUserPresence?>(null));
        }

        if (presence.TenantId != _identityContext.TenantId)
        {
            return Task.FromResult(VKResult.Success<VKUserPresence?>(null));
        }

        return Task.FromResult(VKResult.Success<VKUserPresence?>(presence));
    }

    public Task<VKResult> SavePresenceAsync(
        VKUserPresence presence,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(presence);
        _presences[presence.UserId] = presence;
        return Task.FromResult(VKResult.Success());
    }

    /// <summary>
    /// Seeds a user presence into the in-memory store for local testing.
    /// </summary>
    public InMemoryUserStore Seed(VKUserPresence presence)
    {
        VKGuard.NotNull(presence);
        _presences[presence.UserId] = presence;
        return this;
    }
}
