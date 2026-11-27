using System;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Port for persisting affinity key (RunId / SessionId) to connection bindings across invocations.
/// </summary>
public interface IVKAIAffinityPinStore
{
    ValueTask<string?> GetPinnedConnectionAsync(string affinityKey, CancellationToken cancellationToken = default);

    ValueTask PinIfUnsetAsync(string affinityKey, string connectionId, TimeSpan? ttl = null, CancellationToken cancellationToken = default);

    ValueTask UnpinAsync(string affinityKey, string expectedConnectionId, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
}
