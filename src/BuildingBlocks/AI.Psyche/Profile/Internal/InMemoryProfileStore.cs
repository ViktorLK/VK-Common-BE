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
    private readonly ConcurrentDictionary<VKProfileId, VKProfilePresence> _presences = new();

    public InMemoryProfileStore()
    {
    }

    public Task<VKResult<VKProfilePresence?>> GetProfileAsync(
        VKProfileId profileId,
        CancellationToken cancellationToken = default)
    {
        if (!_presences.TryGetValue(profileId, out var presence))
        {
            return Task.FromResult(VKResult.Success<VKProfilePresence?>(null));
        }

        return Task.FromResult(VKResult.Success<VKProfilePresence?>(presence));
    }

    /// <summary>
    /// Seeds a profile presence into the in-memory store for local testing.
    /// </summary>
    public InMemoryProfileStore Seed(VKProfilePresence presence)
    {
        VKGuard.NotNull(presence);
        _presences[presence.Id] = presence;
        return this;
    }
}
