using VK.Blocks.Core;

namespace VK.Blocks.AI.Connection.Internal;

/// <summary>
/// A resolved, immutable connection contract for an AI provider.
/// This is the final object used by drivers to establish a connection.
/// [AP.01] Immutable Data.
/// </summary>
public sealed record VKAIResolvedContract(
    VKAIProviderType Provider,
    string ModelId,
    VKSensitiveString ApiKey,
    string? Endpoint = null
);
