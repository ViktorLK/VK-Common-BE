using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Profile.Internal;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IVKProfileStore"/> for local development and testing.
/// Follows AP.01 and CS.03.
/// </summary>
internal sealed class InMemoryProfileStore : IVKProfileStore
{
    private readonly ConcurrentDictionary<VKUserId, VKProfilePresence> _presences = new();
    private readonly IVKIdentityContext _identityContext;

    public InMemoryProfileStore(IVKIdentityContext identityContext)
    {
        _identityContext = VKGuard.NotNull(identityContext);
    }

    public Task<VKResult<VKProfilePresence?>> GetProfileAsync(
        VKUserId userId,
        CancellationToken cancellationToken = default)
    {
        if (!_presences.TryGetValue(userId, out var presence))
        {
            return Task.FromResult(VKResult.Success<VKProfilePresence?>(null));
        }

        if (presence.TenantId != _identityContext.TenantId)
        {
            return Task.FromResult(VKResult.Success<VKProfilePresence?>(null));
        }

        return Task.FromResult(VKResult.Success<VKProfilePresence?>(presence));
    }

    public Task<VKResult> SaveProfileAsync(
        VKProfilePresence presence,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(presence);
        _presences[presence.UserId] = presence;
        return Task.FromResult(VKResult.Success());
    }

    /// <summary>
    /// Seeds a profile presence into the in-memory store for local testing.
    /// </summary>
    public InMemoryProfileStore Seed(VKProfilePresence presence)
    {
        VKGuard.NotNull(presence);
        _presences[presence.UserId] = presence;
        return this;
    }
}
