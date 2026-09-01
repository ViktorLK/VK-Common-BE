using Moq;
using VK.Blocks.AI.Psyche.Persona.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Persona;

public sealed class DefaultPersonaFormatterTests : VKUnitTestBase
{
    [Fact]
    public void CanFormat_ReturnsTrue_OnlyForPersonaTier()
    {
        var formatter = new DefaultPersonaFormatter(GetMockObject<IVKPersonaRenderer>());
        var mockMetadata = GetMockObject<IVKFragmentMetadata>();

        formatter.CanFormat(new VKPromptFragment { TierType = VKPromptTierType.Persona, Metadata = mockMetadata, Segment = new VKPromptSegment() }).Should().BeTrue();
        formatter.CanFormat(new VKPromptFragment { TierType = VKPromptTierType.Directive, Metadata = mockMetadata, Segment = new VKPromptSegment() }).Should().BeFalse();
    }

    [Fact]
    public void Format_WithValidPersona_ReturnsFormattedXml()
    {
        GetMock<IVKPersonaRenderer>()
            .Setup(r => r.Render(It.IsAny<VKPersonaAnchor>()))
            .Returns("## Name\nAssistant");

        var formatter = new DefaultPersonaFormatter(GetMockObject<IVKPersonaRenderer>());
        var persona = new VKPersonaAnchorBuilder()
            .WithName("Assistant")
            .WithDescription("Test AI")
            .Build();

        var fragment = new VKPromptFragment
        {
            TierType = VKPromptTierType.Persona,
            Metadata = persona,
            Segment = new VKPromptSegment()
        };
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hi").BuildContext();

        var result = formatter.Format(fragment, context);

        result.Should().BeSuccess();
        result.Value.Should().Contain("<persona>");
        result.Value.Should().Contain("## Name\nAssistant");
        result.Value.Should().Contain("</persona>");
    }

    [Fact]
    public void Format_WithInvalidMetadata_ReturnsFailure()
    {
        var formatter = new DefaultPersonaFormatter(GetMockObject<IVKPersonaRenderer>());
        var mockMetadata = GetMockObject<IVKFragmentMetadata>();
        var fragment = new VKPromptFragment
        {
            TierType = VKPromptTierType.Persona,
            Metadata = mockMetadata,
            Segment = new VKPromptSegment()
        };
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hi").BuildContext();

        var result = formatter.Format(fragment, context);

        result.Should().BeFailure();
    }
}
