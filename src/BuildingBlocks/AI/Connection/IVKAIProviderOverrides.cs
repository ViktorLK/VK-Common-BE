using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines the incremental connection overrides for an AI provider.
/// Usually implemented by Args classes.
/// </summary>
public interface IVKAIProviderOverrides
{
    /// <summary>
    /// Gets the provider type override.
    /// </summary>
    VKAIProviderType? Provider { get; init; }

    /// <summary>
    /// Gets the model identifier override.
    /// </summary>
    string? ModelId { get; init; }

    /// <summary>
    /// Gets the API key override.
    /// [OR.02] PII masked.
    /// </summary>
    VKSensitiveString? ApiKey { get; init; }

    /// <summary>
    /// Gets the service endpoint override.
    /// </summary>
    string? Endpoint { get; init; }
}
