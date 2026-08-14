using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse.Routing.Internal;

/// <summary>
/// Thread-safe in-memory store for AI connections in AI.Synapse.
/// Designed for unit testing, labs, and default fallback without external databases.
/// Follows AP.01 (sealed class default) and CS.03.
/// </summary>
internal sealed class InMemoryAIConnectionStore : IVKAIConnectionStore
{
    private readonly ConcurrentDictionary<string, VKAIConnection> _connections = new(StringComparer.OrdinalIgnoreCase);

    public Task<VKResult<IEnumerable<VKAIConnection>>> GetConnectionListAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(VKResult.Success<IEnumerable<VKAIConnection>>(_connections.Values.ToList()));
    }

    /// <summary>
    /// Seeds an AI connection entry into the in-memory store for local testing or initialization.
    /// </summary>
    public InMemoryAIConnectionStore Seed(VKAIConnection connection)
    {
        VKGuard.NotNull(connection);
        _connections[connection.Id] = connection;
        return this;
    }
}
