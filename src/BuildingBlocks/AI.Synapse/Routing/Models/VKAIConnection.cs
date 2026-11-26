using System.Collections.Generic;
using VK.Blocks.AI;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Domain model representing a physical AI connection channel (OpenAI, Azure, Anthropic, etc.) in the AI.Synapse block.
/// Follows AP.01 (sealed record).
/// </summary>
public sealed record VKAIConnection
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public VKAIProviderType? Provider { get; init; } = VKAIProviderType.OpenAI;
    public string? ModelId { get; init; } = VKAIModelIds.OpenAI.Gpt6Luna;
    public string? DeploymentName { get; init; }
    public string? CredentialRef { get; init; }
    public string? Endpoint { get; init; }
    public string? Region { get; init; }
    public IReadOnlyDictionary<string, string> Tags { get; init; } = new Dictionary<string, string>();
    public bool Enabled { get; init; } = true;
    public int MaxConcurrency { get; init; } = 10;
    public int? RpmLimit { get; init; }
    public int? TpmLimit { get; init; }
}
