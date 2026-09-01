using VK.Blocks.AI.Psyche.Echo.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Echo;

public sealed class EchoRenderersTests : VKUnitTestBase
{
    [Fact]
    public void BracketEchoRenderer_RendersWithBracketFormat()
    {
        // Arrange
        var renderer = new BracketEchoRenderer();
        var trace = new VKEchoTraceBuilder()
            .WithRole(VKChatRole.User)
            .WithContent("Hello")
            .Build();
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();

        // Act
        var result = renderer.Render(trace, context);

        // Assert
        result.Should().Be("[User]: Hello");
    }

    [Fact]
    public void BracketEchoRenderer_WithCustomProfileAndPersona_RendersNames()
    {
        // Arrange
        var renderer = new BracketEchoRenderer();
        var userTrace = new VKEchoTraceBuilder()
            .WithRole(VKChatRole.User)
            .WithContent("User question")
            .Build();
        var assistantTrace = new VKEchoTraceBuilder()
            .WithRole(VKChatRole.Assistant)
            .WithContent("Bot reply")
            .Build();
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();
        context.SetState(new VKProfilePresenceBuilder().WithDisplayName("Alice").Build());
        context.SetState(new VKPersonaAnchorBuilder().WithName("Jarvis").Build());

        // Act
        var userResult = renderer.Render(userTrace, context);
        var assistantResult = renderer.Render(assistantTrace, context);

        // Assert
        userResult.Should().Be("[Alice]: User question");
        assistantResult.Should().Be("[Jarvis]: Bot reply");
    }

    [Fact]
    public void ChatMLEchoRenderer_RendersWithChatMLFormat()
    {
        // Arrange
        var renderer = new ChatMLEchoRenderer();
        var trace = new VKEchoTraceBuilder()
            .WithRole(VKChatRole.Assistant)
            .WithContent("Hi there")
            .Build();
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
        var trace = new VKEchoTraceBuilder()
            .WithRole(VKChatRole.User)
            .WithContent("Prompt")
            .Build();
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();

        // Act
        var result = renderer.Render(trace, context);

        // Assert
        result.Should().Be("User: Prompt");
    }

    [Fact]
    public void HeaderEchoRenderer_WithCustomProfileAndPersona_RendersNames()
    {
        // Arrange
        var renderer = new HeaderEchoRenderer();
        var userTrace = new VKEchoTraceBuilder()
            .WithRole(VKChatRole.User)
            .WithContent("User question")
            .Build();
        var assistantTrace = new VKEchoTraceBuilder()
            .WithRole(VKChatRole.Assistant)
            .WithContent("Bot reply")
            .Build();
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();
        context.SetState(new VKProfilePresenceBuilder().WithDisplayName("Bob").Build());
        context.SetState(new VKPersonaAnchorBuilder().WithName("HAL").Build());

        // Act
        var userResult = renderer.Render(userTrace, context);
        var assistantResult = renderer.Render(assistantTrace, context);

        // Assert
        userResult.Should().Be("Bob: User question");
        assistantResult.Should().Be("HAL: Bot reply");
    }

    [Fact]
    public void RawEchoRenderer_RendersContentOnly()
    {
        // Arrange
        var renderer = new RawEchoRenderer();
        var trace = new VKEchoTraceBuilder()
            .WithRole(VKChatRole.User)
            .WithContent("Raw content")
            .Build();
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
        var trace = new VKEchoTraceBuilder()
            .WithRole(VKChatRole.User)
            .WithContent("Xml payload")
            .Build();
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();

        // Act
        var result = renderer.Render(trace, context);

        // Assert
        result.Should().Be("<message role=\"user\">Xml payload</message>");
    }
}
