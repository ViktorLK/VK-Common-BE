using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Domain model representing a physical AI connection channel (OpenAI, Azure, Anthropic, etc.) in the AI.Synapse block.
/// Follows AP.01 (sealed record) and implements <see cref="IVKAIProviderOptions"/> and <see cref="IVKTenantScoped"/>.
/// </summary>
public sealed record VKAIConnection : IVKAIProviderOptions, IVKTenantScoped
{
    public required string Id { get; init; }
    public VKTenantId TenantId { get; init; } = VKTenantId.Default;
    public required string Name { get; init; }
    public VKAIProviderType? Provider { get; init; } = VKAIProviderType.OpenAI;
    public string? ModelId { get; init; } = VKAIModelIds.OpenAI.Gpt4OMini;
    public VKSensitiveString? ApiKey { get; init; }
    public string? Endpoint { get; init; }
    public bool IsDefault { get; init; }
    public int MaxConcurrency { get; init; } = 10;
}
