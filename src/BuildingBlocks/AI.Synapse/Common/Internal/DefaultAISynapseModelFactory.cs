using System;
using System.Collections.Generic;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse.Common.Internal;

/// <summary>
/// Default implementation of <see cref="IVKAISynapseModelFactory"/> which binds current <see cref="IVKIdentityContext"/>,
/// <see cref="IVKGuidGenerator"/> (CS.06), and <see cref="TimeProvider"/> (CS.06) to Synapse models.
/// Follows AP.01.
/// </summary>
internal sealed class DefaultAISynapseModelFactory : IVKAISynapseModelFactory
{
    private readonly IVKIdentityContext _identityContext;
    private readonly IVKGuidGenerator _guidGenerator;
    private readonly TimeProvider _timeProvider;

    public DefaultAISynapseModelFactory(
        IVKIdentityContext identityContext,
        IVKGuidGenerator guidGenerator,
        TimeProvider timeProvider)
    {
        _identityContext = VKGuard.NotNull(identityContext);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
        _timeProvider = VKGuard.NotNull(timeProvider);
    }

    public VKAIConnection CreateConnection(
        string name,
        VKAIProviderType provider = VKAIProviderType.OpenAI,
        string modelId = VKAIModelIds.OpenAI.Gpt4OMini,
        string? apiKey = null,
        string? endpoint = null,
        bool isDefault = false,
        int maxConcurrency = 10,
        VKTenantId? tenantId = null)
    {
        return CreateConnection(
            _guidGenerator.Create().ToString(),
            name,
            provider,
            modelId,
            apiKey,
            endpoint,
            isDefault,
            maxConcurrency,
            tenantId);
    }

    public VKAIConnection CreateConnection(
        string id,
        string name,
        VKAIProviderType provider = VKAIProviderType.OpenAI,
        string modelId = VKAIModelIds.OpenAI.Gpt4OMini,
        string? apiKey = null,
        string? endpoint = null,
        bool isDefault = false,
        int maxConcurrency = 10,
        VKTenantId? tenantId = null)
    {
        VKGuard.NotNullOrWhiteSpace(id);
        VKGuard.NotNullOrWhiteSpace(name);

        VKSensitiveString? sensitiveKey = null;
        if (!string.IsNullOrEmpty(apiKey))
        {
            sensitiveKey = new VKSensitiveString(apiKey);
        }

        return new VKAIConnection
        {
            Id = id,
            TenantId = tenantId ?? _identityContext.TenantId,
            Name = name,
            Provider = provider,
            ModelId = modelId,
            ApiKey = sensitiveKey,
            Endpoint = endpoint,
            IsDefault = isDefault,
            MaxConcurrency = maxConcurrency > 0 ? maxConcurrency : 10
        };
    }

    public VKAIRouteArgs CreateRouteArgs(
        string operationKey = "AI.Execute",
        VKAIProviderType? preferredProvider = null,
        string? preferredModelId = null)
    {
        return new VKAIRouteArgs
        {
            OperationKey = operationKey,
            PreferredProvider = preferredProvider,
            PreferredModelId = preferredModelId
        };
    }

    public VKAIUsageMetrics CreateUsageMetrics(
        long promptTokens,
        long completionTokens,
        TimeSpan duration,
        double estimatedCost = 0.0)
    {
        return new VKAIUsageMetrics
        {
            PromptTokens = promptTokens,
            CompletionTokens = completionTokens,
            Duration = duration,
            EstimatedCost = estimatedCost
        };
    }
}
