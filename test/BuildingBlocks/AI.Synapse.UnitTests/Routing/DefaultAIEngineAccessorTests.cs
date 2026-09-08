using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VK.Blocks.AI;
using VK.Blocks.AI.Synapse.Routing.Internal;
using Xunit;

namespace VK.Blocks.AI.Synapse.UnitTests.Routing;

/// <summary>
/// Unit tests for <see cref="DefaultAIEngineAccessor"/>.
/// Follows AP.01, CS.01, and DL.01.
/// </summary>
public sealed class DefaultAIEngineAccessorTests
{
    [Fact]
    public void GetChatEngine_WithProviderType_ResolvesKeyedEngine()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockEngine = new Mock<IVKChatEngine>();

        services.AddKeyedSingleton<IVKChatEngine>(VKAIProviderType.OpenAI, mockEngine.Object);
        var sp = services.BuildServiceProvider();
        var accessor = new DefaultAIEngineAccessor(sp);

        // Act
        var resolved = accessor.GetChatEngine(VKAIProviderType.OpenAI);

        // Assert
        resolved.Should().NotBeNull();
        resolved.Should().BeSameAs(mockEngine.Object);
    }

    [Fact]
    public void GetChatEngine_WithStringName_ResolvesKeyedEngine()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockEngine = new Mock<IVKChatEngine>();

        services.AddKeyedSingleton<IVKChatEngine>("Anthropic", mockEngine.Object);
        var sp = services.BuildServiceProvider();
        var accessor = new DefaultAIEngineAccessor(sp);

        // Act
        var resolved = accessor.GetChatEngine("Anthropic");

        // Assert
        resolved.Should().NotBeNull();
        resolved.Should().BeSameAs(mockEngine.Object);
    }

    [Fact]
    public void GetEngine_WithCustomKey_ResolvesGenericService()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockEngine = new Mock<IVKChatEngine>();

        services.AddKeyedSingleton<IVKChatEngine>("azure-chat", mockEngine.Object);
        var sp = services.BuildServiceProvider();
        var accessor = new DefaultAIEngineAccessor(sp);

        // Act
        var resolved = accessor.GetEngine<IVKChatEngine>("azure-chat");

        // Assert
        resolved.Should().NotBeNull();
        resolved.Should().BeSameAs(mockEngine.Object);
    }
}
