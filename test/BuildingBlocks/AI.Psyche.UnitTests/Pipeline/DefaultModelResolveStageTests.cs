using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using VK.Blocks.AI;
using VK.Blocks.AI.Psyche.Pipeline.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Pipeline;

/// <summary>
/// Unit tests for <see cref="DefaultModelResolveStage"/>.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public sealed class DefaultModelResolveStageTests : VKUnitTestBase
{
    private readonly DefaultModelResolveStage _stage;

    public DefaultModelResolveStageTests()
    {
        _stage = new DefaultModelResolveStage(GetMockObject<IVKVKAIModelCatalog>());
    }

    [Fact]
    // [DL.01] Method_Scenario_Expected naming convention
    public void Constructor_WhenCatalogNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new DefaultModelResolveStage(null!));
    }

    [Fact]
    public async Task ExecuteAsync_WhenContextNull_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _stage.ExecuteAsync(null!));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var (context, _) = new VKPsycheRequestBuilder().BuildContext();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => _stage.ExecuteAsync(context, cts.Token));
    }

    [Fact]
    public async Task ExecuteAsync_WhenProviderUnconfiguredAndNotWeaveOnly_ReturnsFailure()
    {
        // Arrange
        var (context, _) = new VKPsycheRequestBuilder()
            .WithWeaveOnly(false)
            .BuildContext();

        // Act
        var result = await _stage.ExecuteAsync(context);

        // Assert
        result.Should().BeFailure(VKPipelineErrors.ProviderNotConfigured);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProviderUnconfiguredAndWeaveOnly_ReturnsSuccess()
    {
        // Arrange
        var (context, _) = new VKPsycheRequestBuilder()
            .WithWeaveOnly(true)
            .BuildContext();

        // Act
        var result = await _stage.ExecuteAsync(context);

        // Assert
        result.Should().BeSuccess();
        context.State<VKAIModelMetadata>().Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenModelUnconfiguredAndNotWeaveOnly_ReturnsFailure()
    {
        // Arrange
        var (context, _) = new VKPsycheRequestBuilder()
            .WithWeaveOnly(false)
            .WithRequestArgs(new VKChatArgs { Provider = VKAIProviderType.OpenAI, ModelId = null })
            .BuildContext();

        // Act
        var result = await _stage.ExecuteAsync(context);

        // Assert
        result.Should().BeFailure(VKPipelineErrors.ModelNotConfigured);
    }

    [Fact]
    public async Task ExecuteAsync_WhenModelUnconfiguredAndWeaveOnly_ReturnsSuccess()
    {
        // Arrange
        var (context, _) = new VKPsycheRequestBuilder()
            .WithWeaveOnly(true)
            .WithRequestArgs(new VKChatArgs { Provider = VKAIProviderType.OpenAI, ModelId = "" })
            .BuildContext();

        // Act
        var result = await _stage.ExecuteAsync(context);

        // Assert
        result.Should().BeSuccess();
        context.State<VKAIModelMetadata>().Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenArgsConfigured_ResolvesModelMetadataAndSetsState()
    {
        // Arrange
        var expectedMetadata = new VKAIModelMetadata
        {
            Provider = VKAIProviderType.OpenAI,
            ModelId = "gpt-4o",
            ContextWindowSize = 128000,
            MaxOutputTokens = 4096
        };

        GetMock<IVKVKAIModelCatalog>()
            .Setup(c => c.GetAIModelMetadata(VKAIProviderType.OpenAI, "gpt-4o"))
            .Returns(expectedMetadata);

        var (context, _) = new VKPsycheRequestBuilder()
            .WithRequestArgs(new VKChatArgs { Provider = VKAIProviderType.OpenAI, ModelId = "gpt-4o" })
            .BuildContext();

        // Act
        var result = await _stage.ExecuteAsync(context);

        // Assert
        result.Should().BeSuccess();
        context.State<VKAIModelMetadata>().Should().BeSameAs(expectedMetadata);
    }

    [Fact]
    public async Task ExecuteAsync_WhenArgsOmitted_FallsBackToAmbientChatOptionsFromServices()
    {
        // Arrange
        var expectedMetadata = new VKAIModelMetadata
        {
            Provider = VKAIProviderType.Anthropic,
            ModelId = "claude-3-5-sonnet",
            ContextWindowSize = 200000,
            MaxOutputTokens = 8192
        };

        GetMock<IVKVKAIModelCatalog>()
            .Setup(c => c.GetAIModelMetadata(VKAIProviderType.Anthropic, "claude-3-5-sonnet"))
            .Returns(expectedMetadata);

        var services = new ServiceCollection();
        services.AddSingleton(new VKChatOptions
        {
            Provider = VKAIProviderType.Anthropic,
            ModelId = "claude-3-5-sonnet"
        });

        var (context, _) = new VKPsycheRequestBuilder()
            .BuildContext(services);

        // Act
        var result = await _stage.ExecuteAsync(context);

        // Assert
        result.Should().BeSuccess();
        context.State<VKAIModelMetadata>().Should().BeSameAs(expectedMetadata);
    }

    [Fact]
    public async Task ExecuteAsync_WhenArgsOmitted_FallsBackToOptionsWrapper()
    {
        // Arrange
        var expectedMetadata = new VKAIModelMetadata
        {
            Provider = VKAIProviderType.Google,
            ModelId = "gemini-2.0-flash",
            ContextWindowSize = 1000000,
            MaxOutputTokens = 8192
        };

        GetMock<IVKVKAIModelCatalog>()
            .Setup(c => c.GetAIModelMetadata(VKAIProviderType.Google, "gemini-2.0-flash"))
            .Returns(expectedMetadata);

        var services = new ServiceCollection();
        services.AddSingleton<IOptions<VKChatOptions>>(Options.Create(new VKChatOptions
        {
            Provider = VKAIProviderType.Google,
            ModelId = "gemini-2.0-flash"
        }));

        var (context, _) = new VKPsycheRequestBuilder()
            .BuildContext(services);

        // Act
        var result = await _stage.ExecuteAsync(context);

        // Assert
        result.Should().BeSuccess();
        context.State<VKAIModelMetadata>().Should().BeSameAs(expectedMetadata);
    }
}
