using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI;

/// <summary>
/// Specific configuration for a single AI provider instance.
/// </summary>
public sealed record VKAIMEAIProviderSettings
{
    /// <summary>
    /// The AI provider platform type (OpenAI, AzureOpenAI, Ollama, Google, Custom).
    /// </summary>
    public VKAIProviderType Provider { get; init; } = VKAIProviderType.OpenAI;

    /// <summary>
    /// The default model ID for chat completions.
    /// </summary>
    public string? ChatModelId { get; init; }

    /// <summary>
    /// The default model ID for embeddings.
    /// </summary>
    public string? EmbeddingModelId { get; init; }

    /// <summary>
    /// The default model ID for text-to-speech.
    /// </summary>
    public string? SpeechModelId { get; init; }

    /// <summary>
    /// The default model ID for speech-to-text transcription.
    /// </summary>
    public string? TranscriptionModelId { get; init; }

    /// <summary>
    /// The default model ID for image generation.
    /// </summary>
    public string? ImageModelId { get; init; }

    /// <summary>
    /// API Key for the provider (if required).
    /// </summary>
    public VKSensitiveString? ApiKey { get; init; }

    /// <summary>
    /// Custom API endpoint (required for Azure OpenAI and Ollama).
    /// </summary>
    public string? Endpoint { get; init; }

    /// <summary>
    /// Organization ID (optional for OpenAI).
    /// </summary>
    public string? OrganizationId { get; init; }

    /// <summary>
    /// Project ID (optional for OpenAI).
    /// </summary>
    public string? ProjectId { get; init; }

    /// <summary>
    /// Optional HTTP timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 60;

    /// <summary>
    /// Optional mapping from logical model IDs to provider-specific deployment names (e.g. Azure OpenAI deployments).
    /// </summary>
    public Dictionary<string, string> Deployments { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Resolves a model ID or mapped deployment name.
    /// </summary>
    public string? ResolveModelOrDeployment(string? modelId)
    {
        if (string.IsNullOrWhiteSpace(modelId))
        {
            return null;
        }

        return Deployments.TryGetValue(modelId, out var deployment) && !string.IsNullOrWhiteSpace(deployment)
            ? deployment
            : modelId;
    }
}
