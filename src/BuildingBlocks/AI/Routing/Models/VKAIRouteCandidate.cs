using System;

namespace VK.Blocks.AI;

/// <summary>
/// Represents a resolved AI connection candidate returned by a routing policy.
/// </summary>
// [AP.01]
public sealed record VKAIRouteCandidate
{
    /// <summary>
    /// Gets the unique connection identifier.
    /// </summary>
    public required string ConnectionId { get; init; }

    /// <summary>
    /// Gets the AI provider name (e.g., OpenAI, AzureOpenAI, Anthropic).
    /// </summary>
    public required string Provider { get; init; }

    /// <summary>
    /// Gets the target model identifier.
    /// </summary>
    public required string ModelId { get; init; }

    /// <summary>
    /// Gets the optional deployment name.
    /// </summary>
    public string? DeploymentName { get; init; }

    /// <summary>
    /// Gets the optional endpoint URI.
    /// </summary>
    public Uri? Endpoint { get; init; }

    /// <summary>
    /// Gets the optional credential reference key.
    /// </summary>
    public string? CredentialRef { get; init; }

    /// <summary>
    /// Gets the optional cloud region identifier.
    /// </summary>
    public string? Region { get; init; }
}
