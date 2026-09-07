using System;
using FluentAssertions;
using Moq;
using VK.Blocks.AI;
using VK.Blocks.AI.Synapse.Common.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Synapse.UnitTests.Common;

/// <summary>
/// Unit tests for <see cref="DefaultAISynapseModelFactory"/>.
/// Follows AP.01, CS.01, CS.06, and DL.01.
/// </summary>
public sealed class DefaultAISynapseModelFactoryTests
{
    private readonly Mock<IVKIdentityContext> _identityContextMock;
    private readonly Mock<IVKGuidGenerator> _guidGeneratorMock;
    private readonly TimeProvider _timeProvider;
    private readonly Guid _generatedGuid;
    private readonly VKTenantId _defaultTenantId;

    public DefaultAISynapseModelFactoryTests()
    {
        _generatedGuid = Guid.NewGuid();
        _defaultTenantId = new VKTenantId(Guid.NewGuid());

        _identityContextMock = new Mock<IVKIdentityContext>();
        _identityContextMock.Setup(i => i.TenantId).Returns(_defaultTenantId);

        _guidGeneratorMock = new Mock<IVKGuidGenerator>();
        _guidGeneratorMock.Setup(g => g.Create()).Returns(_generatedGuid);

        _timeProvider = TimeProvider.System;
    }

    [Fact]
    public void CreateConnection_WithoutId_GeneratesGuidAndUsesIdentityTenant()
    {
        // Arrange
        var factory = new DefaultAISynapseModelFactory(
            _identityContextMock.Object,
            _guidGeneratorMock.Object,
            _timeProvider);

        // Act
        var connection = factory.CreateConnection(
            name: "Default OpenAI",
            provider: VKAIProviderType.OpenAI,
            modelId: VKAIModelIds.OpenAI.Gpt4O,
            apiKey: "sk-test-secret",
            endpoint: "https://api.openai.com/v1",
            isDefault: true,
            maxConcurrency: 15);

        // Assert
        connection.Id.Should().Be(_generatedGuid.ToString());
        connection.TenantId.Should().Be(_defaultTenantId);
        connection.Name.Should().Be("Default OpenAI");
        connection.Provider.Should().Be(VKAIProviderType.OpenAI);
        connection.ModelId.Should().Be(VKAIModelIds.OpenAI.Gpt4O);
        connection.ApiKey.Should().NotBeNull();
        connection.ApiKey!.Value.Reveal().Should().Be("sk-test-secret");
        connection.Endpoint.Should().Be("https://api.openai.com/v1");
        connection.IsDefault.Should().BeTrue();
        connection.MaxConcurrency.Should().Be(15);
    }

    [Fact]
    public void CreateConnection_WithCustomIdAndTenant_AssignsExplicitValues()
    {
        // Arrange
        var customTenantId = new VKTenantId(Guid.NewGuid());
        var factory = new DefaultAISynapseModelFactory(
            _identityContextMock.Object,
            _guidGeneratorMock.Object,
            _timeProvider);

        // Act
        var connection = factory.CreateConnection(
            id: "conn-azure-custom",
            name: "Azure Claude",
            provider: VKAIProviderType.Anthropic,
            modelId: VKAIModelIds.Anthropic.Claude35Sonnet,
            apiKey: null,
            endpoint: null,
            isDefault: false,
            maxConcurrency: 0,
            tenantId: customTenantId);

        // Assert
        connection.Id.Should().Be("conn-azure-custom");
        connection.TenantId.Should().Be(customTenantId);
        connection.Name.Should().Be("Azure Claude");
        connection.Provider.Should().Be(VKAIProviderType.Anthropic);
        connection.ModelId.Should().Be(VKAIModelIds.Anthropic.Claude35Sonnet);
        connection.ApiKey.Should().BeNull();
        connection.Endpoint.Should().BeNull();
        connection.IsDefault.Should().BeFalse();
        connection.MaxConcurrency.Should().Be(10); // Fallback to 10 when maxConcurrency <= 0
    }

    [Fact]
    public void CreateRouteArgs_ReturnsConfiguredRouteArgs()
    {
        // Arrange
        var factory = new DefaultAISynapseModelFactory(
            _identityContextMock.Object,
            _guidGeneratorMock.Object,
            _timeProvider);

        // Act
        var args = factory.CreateRouteArgs(
            operationKey: "Chat.Summarize",
            preferredProvider: VKAIProviderType.Google,
            preferredModelId: VKAIModelIds.Google.Gemini20Flash);

        // Assert
        args.OperationKey.Should().Be("Chat.Summarize");
        args.PreferredProvider.Should().Be(VKAIProviderType.Google);
        args.PreferredModelId.Should().Be(VKAIModelIds.Google.Gemini20Flash);
    }

    [Fact]
    public void CreateUsageMetrics_ReturnsValidMetrics()
    {
        // Arrange
        var factory = new DefaultAISynapseModelFactory(
            _identityContextMock.Object,
            _guidGeneratorMock.Object,
            _timeProvider);

        var duration = TimeSpan.FromMilliseconds(450);

        // Act
        var metrics = factory.CreateUsageMetrics(
            promptTokens: 1200,
            completionTokens: 300,
            duration: duration,
            estimatedCost: 0.0045);

        // Assert
        metrics.PromptTokens.Should().Be(1200);
        metrics.CompletionTokens.Should().Be(300);
        metrics.Duration.Should().Be(duration);
        metrics.EstimatedCost.Should().Be(0.0045);
    }
}
