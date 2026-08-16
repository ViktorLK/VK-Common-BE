using System;
using FluentAssertions;
using VK.Blocks.AI.Psyche.Echo.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Echo;

public sealed class EchoRenderersTests
{
    [Fact]
    public void BracketEchoRenderer_RendersWithBracketFormat()
    {
        // Arrange
        var renderer = new BracketEchoRenderer();
        var trace = new VKEchoTrace
        {
            Id = new VKEchoId(Guid.NewGuid()),
            SessionId = new VKSessionId(Guid.NewGuid()),
            TenantId = VKTenantId.Default,
            Role = VKChatRole.User,
            Content = "Hello"
        };
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();

        // Act
        var result = renderer.Render(trace, context);

        // Assert
        result.Should().Be("[User]: Hello");
    }

    [Fact]
    public void ChatMLEchoRenderer_RendersWithChatMLFormat()
    {
        // Arrange
        var renderer = new ChatMLEchoRenderer();
        var trace = new VKEchoTrace
        {
            Id = new VKEchoId(Guid.NewGuid()),
            SessionId = new VKSessionId(Guid.NewGuid()),
            TenantId = VKTenantId.Default,
            Role = VKChatRole.Assistant,
            Content = "Hi there"
        };
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();

        // Act
        var result = renderer.Render(trace, context);

        // Assert
        result.Should().Contain("<|im_start|>assistant");
        result.Should().Contain("Hi there");
        result.Should().Contain("<|im_end|>");
    }

    [Fact]
    public void HeaderEchoRenderer_RendersWithHeaderFormat()
    {
        // Arrange
        var renderer = new HeaderEchoRenderer();
        var trace = new VKEchoTrace
        {
            Id = new VKEchoId(Guid.NewGuid()),
            SessionId = new VKSessionId(Guid.NewGuid()),
            TenantId = VKTenantId.Default,
            Role = VKChatRole.User,
            Content = "Prompt"
        };
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();

        // Act
        var result = renderer.Render(trace, context);

        // Assert
        result.Should().Be("User: Prompt");
    }

    [Fact]
    public void RawEchoRenderer_RendersContentOnly()
    {
        // Arrange
        var renderer = new RawEchoRenderer();
        var trace = new VKEchoTrace
        {
            Id = new VKEchoId(Guid.NewGuid()),
            SessionId = new VKSessionId(Guid.NewGuid()),
            TenantId = VKTenantId.Default,
            Role = VKChatRole.User,
            Content = "Raw content"
        };
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();

        // Act
        var result = renderer.Render(trace, context);

        // Assert
        result.Should().Be("Raw content");
    }

    [Fact]
    public void XmlEchoRenderer_RendersWithXmlTags()
    {
        // Arrange
        var renderer = new XmlEchoRenderer();
        var trace = new VKEchoTrace
        {
            Id = new VKEchoId(Guid.NewGuid()),
            SessionId = new VKSessionId(Guid.NewGuid()),
            TenantId = VKTenantId.Default,
            Role = VKChatRole.User,
            Content = "Xml payload"
        };
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();

        // Act
        var result = renderer.Render(trace, context);

        // Assert
        result.Should().Be("<message role=\"user\">Xml payload</message>");
    }
}
