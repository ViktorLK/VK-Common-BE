using System;
using VK.Blocks.AI;
using VK.Blocks.AI.Synapse;
using VK.Blocks.Core;
using VK.Blocks.Testing.Builders;

namespace VK.Blocks.AI.Synapse.UnitTests.Builders;

/// <summary>
/// Test data builder for constructing <see cref="VKAIConnection"/> instances.
/// Follows AP.01.
/// </summary>
public sealed class VKAIConnectionBuilder : VKTestDataBuilder<VKAIConnection>
{
    private string _id = Guid.NewGuid().ToString();
    private VKTenantId _tenantId = new(Guid.NewGuid());
    private string _name = "Test-Connection";
    private VKAIProviderType? _provider = VKAIProviderType.OpenAI;
    private string? _modelId = VKAIModelIds.OpenAI.Gpt4OMini;
    private VKSensitiveString? _apiKey = new("sk-test-key-12345");
    private string? _endpoint = "https://api.openai.com/v1";
    private bool _isDefault;
    private int _maxConcurrency = 10;

    public VKAIConnectionBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public VKAIConnectionBuilder WithTenantId(VKTenantId tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    public VKAIConnectionBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public VKAIConnectionBuilder WithProvider(VKAIProviderType? provider)
    {
        _provider = provider;
        return this;
    }

    public VKAIConnectionBuilder WithModelId(string? modelId)
    {
        _modelId = modelId;
        return this;
    }

    public VKAIConnectionBuilder WithApiKey(string? apiKey)
    {
        _apiKey = apiKey != null ? new VKSensitiveString(apiKey) : null;
        return this;
    }

    public VKAIConnectionBuilder WithEndpoint(string? endpoint)
    {
        _endpoint = endpoint;
        return this;
    }

    public VKAIConnectionBuilder WithDefault(bool isDefault = true)
    {
        _isDefault = isDefault;
        return this;
    }

    public VKAIConnectionBuilder WithMaxConcurrency(int maxConcurrency)
    {
        _maxConcurrency = maxConcurrency;
        return this;
    }

    protected override VKAIConnection CreateDefault()
    {
        return new VKAIConnection
        {
            Id = _id,
            TenantId = _tenantId,
            Name = _name,
            Provider = _provider,
            ModelId = _modelId,
            ApiKey = _apiKey,
            Endpoint = _endpoint,
            IsDefault = _isDefault,
            MaxConcurrency = _maxConcurrency
        };
    }
}
