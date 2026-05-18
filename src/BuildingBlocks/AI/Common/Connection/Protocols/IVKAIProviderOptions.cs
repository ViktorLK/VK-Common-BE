using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines the baseline connection settings for an AI provider.
/// Usually implemented by Options classes.
/// </summary>
public interface IVKAIProviderOptions
{
    /// <summary>
    /// Gets the provider type.
    /// </summary>
    VKAIProviderType? Provider { get; init; }

    /// <summary>
    /// Gets the model identifier.
    /// </summary>
    string? ModelId { get; init; }

    /// <summary>
    /// Gets the API key.
    /// [OR.02] PII masked.
    /// </summary>
    VKSensitiveString? ApiKey { get; init; }

    /// <summary>
    /// Gets the service endpoint.
    /// </summary>
    string? Endpoint { get; init; }
}
