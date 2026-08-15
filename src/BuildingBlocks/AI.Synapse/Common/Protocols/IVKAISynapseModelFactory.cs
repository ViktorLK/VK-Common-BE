using System;
using System.Collections.Generic;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Domain model factory for creating AI.Synapse models automatically bound to ambient identity context, IVKGuidGenerator, and TimeProvider.
/// Follows AP.01, AP.03, CS.06.
/// </summary>
public interface IVKAISynapseModelFactory
{
    /// <summary>
    /// Creates a new <see cref="VKAIConnection"/> with an automatically generated ID.
    /// </summary>
    VKAIConnection CreateConnection(
        string name,
        VKAIProviderType provider = VKAIProviderType.OpenAI,
        string modelId = VKAIModelIds.OpenAI.Gpt4OMini,
        string? apiKey = null,
        string? endpoint = null,
        bool isDefault = false,
        int maxConcurrency = 10,
        VKTenantId? tenantId = null);

    /// <summary>
    /// Creates a new <see cref="VKAIConnection"/> with an explicitly specified ID.
    /// </summary>
    VKAIConnection CreateConnection(
        string id,
        string name,
        VKAIProviderType provider = VKAIProviderType.OpenAI,
        string modelId = VKAIModelIds.OpenAI.Gpt4OMini,
        string? apiKey = null,
        string? endpoint = null,
        bool isDefault = false,
        int maxConcurrency = 10,
        VKTenantId? tenantId = null);

    /// <summary>
    /// Creates a new <see cref="VKAIRouteArgs"/> with optional provider and model preferences.
    /// </summary>
    VKAIRouteArgs CreateRouteArgs(
        string operationKey = "AI.Execute",
        VKAIProviderType? preferredProvider = null,
        string? preferredModelId = null);

    /// <summary>
    /// Creates a new <see cref="VKAIUsageMetrics"/> capturing execution measurements.
    /// </summary>
    VKAIUsageMetrics CreateUsageMetrics(
        long promptTokens,
        long completionTokens,
        TimeSpan duration,
        double estimatedCost = 0.0);
}
