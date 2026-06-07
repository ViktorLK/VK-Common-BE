using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration for cross-provider fallback in the Chat feature.
/// </summary>
public sealed record VKChatFallbackConfig : IVKAIProviderOptions
{
    /// <inheritdoc />
    public VKAIProviderType? Provider { get; init; }

    /// <inheritdoc />
    public string? ModelId { get; init; }

    /// <inheritdoc />
    public VKSensitiveString? ApiKey { get; init; }

    /// <inheritdoc />
    public string? Endpoint { get; init; }
}
