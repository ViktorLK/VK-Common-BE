using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using VK.Blocks.AI.Psyche.Persona.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Persona;

public sealed class DefaultPersonaFormatterTests
{
    [Fact]
    public void CanFormat_ReturnsTrue_OnlyForPersonaTier()
    {
        var rendererMock = new Mock<IVKPersonaRenderer>();
        var formatter = new DefaultPersonaFormatter(rendererMock.Object);
        var mockMetadata = new Mock<IVKFragmentMetadata>().Object;

        formatter.CanFormat(new VKPromptFragment { TierType = VKPromptTierType.Persona, Metadata = mockMetadata, Segment = new VKPromptSegment() }).Should().BeTrue();
        formatter.CanFormat(new VKPromptFragment { TierType = VKPromptTierType.Directive, Metadata = mockMetadata, Segment = new VKPromptSegment() }).Should().BeFalse();
    }

    [Fact]
    public void Format_WithValidPersona_ReturnsFormattedXml()
    {
        var rendererMock = new Mock<IVKPersonaRenderer>();
        rendererMock.Setup(r => r.Render(It.IsAny<VKPersonaAnchor>())).Returns("## Name\nAssistant");

        var formatter = new DefaultPersonaFormatter(rendererMock.Object);
        var persona = new VKPersonaAnchor
        {
            Id = new VKPersonaId(Guid.NewGuid()),
            TenantId = VKTenantId.Default,
            Name = "Assistant",
            Description = "Test AI"
        };
        var fragment = new VKPromptFragment
        {
            TierType = VKPromptTierType.Persona,
            Metadata = persona,
            Segment = new VKPromptSegment()
        };
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hi").BuildContext();

        var result = formatter.Format(fragment, context);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("<persona>");
        result.Value.Should().Contain("## Name\nAssistant");
        result.Value.Should().Contain("</persona>");
    }

    [Fact]
    public void Format_WithInvalidMetadata_ReturnsFailure()
    {
        var rendererMock = new Mock<IVKPersonaRenderer>();
        var formatter = new DefaultPersonaFormatter(rendererMock.Object);
        var mockMetadata = new Mock<IVKFragmentMetadata>().Object;
        var fragment = new VKPromptFragment
        {
            TierType = VKPromptTierType.Persona,
            Metadata = mockMetadata,
            Segment = new VKPromptSegment()
        };
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hi").BuildContext();

        var result = formatter.Format(fragment, context);

        result.IsFailure.Should().BeTrue();
    }
}
