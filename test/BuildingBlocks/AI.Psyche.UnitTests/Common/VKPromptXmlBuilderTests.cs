using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Common;

/// <summary>
/// Unit tests for <see cref="VKPromptXmlBuilder"/>.
/// Follows AP.01, CS.01, and DL.01 rules.
/// </summary>
public sealed class VKPromptXmlBuilderTests : VKUnitTestBase
{
    [Fact]
    // [DL.01] Method_Scenario_Expected naming convention
    public void Wrap_SingleContentWithValidInputs_ReturnsFormattedXml()
    {
        // Act
        var result = VKPromptXmlBuilder.Wrap("instruction", "Follow these rules.");

        // Assert
        result.Should().Be("<instruction>\r\nFollow these rules.\r\n</instruction>");
    }

    [Fact]
    public void Wrap_SingleContentWithAttributes_ReturnsFormattedXmlWithAttributes()
    {
        // Arrange
        var attributes = new Dictionary<string, string>
        {
            ["priority"] = "high",
            ["tier"] = "system"
        };

        // Act
        var result = VKPromptXmlBuilder.Wrap("prompt", "Hello world", attributes);

        // Assert
        result.Should().Contain("<prompt priority=\"high\" tier=\"system\">");
        result.Should().Contain("Hello world");
        result.Should().EndWith("</prompt>");
    }

    [Fact]
    public void Wrap_SingleContentWhenContentIsNullOrWhitespace_ReturnsEmptyString()
    {
        // Act
        var res1 = VKPromptXmlBuilder.Wrap("test", (string)null!);
        var res2 = VKPromptXmlBuilder.Wrap("test", "");
        var res3 = VKPromptXmlBuilder.Wrap("test", "   \t\r\n  ");

        // Assert
        res1.Should().BeEmpty();
        res2.Should().BeEmpty();
        res3.Should().BeEmpty();
    }

    [Fact]
    public void Wrap_SingleContentWhenTagNameIsInvalid_ThrowsException()
    {
        Assert.ThrowsAny<ArgumentException>(() => VKPromptXmlBuilder.Wrap(null!, "content"));
        Assert.Throws<ArgumentException>(() => VKPromptXmlBuilder.Wrap("", "content"));
        Assert.Throws<ArgumentException>(() => VKPromptXmlBuilder.Wrap("   ", "content"));
    }

    [Fact]
    public void Wrap_MultipleItemsWithValidInputs_ReturnsCoalescedXml()
    {
        // Arrange
        var items = new[] { "Rule 1", "Rule 2", "Rule 3" };

        // Act
        var result = VKPromptXmlBuilder.Wrap("rules", items, separator: "\n---\n");

        // Assert
        result.Should().StartWith("<rules>\r\n");
        result.Should().Contain("Rule 1");
        result.Should().Contain("\n---\nRule 2");
        result.Should().Contain("\n---\nRule 3");
        result.Should().EndWith("</rules>");
    }

    [Fact]
    public void Wrap_MultipleItemsWithAttributes_IncludesRootAttributes()
    {
        // Arrange
        var items = new[] { "Item 1" };
        var attrs = new Dictionary<string, string> { ["type"] = "batch" };

        // Act
        var result = VKPromptXmlBuilder.Wrap("items", items, attributes: attrs);

        // Assert
        result.Should().StartWith("<items type=\"batch\">");
        result.Should().Contain("Item 1");
        result.Should().EndWith("</items>");
    }

    [Fact]
    public void Wrap_MultipleItemsFiltersOutNullOrWhitespaceEntries()
    {
        // Arrange
        var items = new[] { "Item 1", null!, "   ", "Item 2" };

        // Act
        var result = VKPromptXmlBuilder.Wrap("items", items);

        // Assert
        result.Should().Contain("Item 1");
        result.Should().Contain("Item 2");
        result.Should().NotContain("null");
    }

    [Fact]
    public void Wrap_MultipleItemsWhenAllEmpty_ReturnsEmptyString()
    {
        // Arrange
        var items = new[] { "   ", "", null! };

        // Act
        var result = VKPromptXmlBuilder.Wrap("items", items);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Wrap_MultipleItemsWhenTagNameOrItemsNull_ThrowsException()
    {
        Assert.ThrowsAny<ArgumentException>(() => VKPromptXmlBuilder.Wrap(null!, new[] { "a" }));
        Assert.Throws<ArgumentNullException>(() => VKPromptXmlBuilder.Wrap("tag", (IEnumerable<string>)null!));
    }

    [Fact]
    public void WrapSelfClosing_WithValidAttributes_ReturnsFormattedSelfClosingTag()
    {
        // Arrange
        var attrs = new Dictionary<string, string>
        {
            ["id"] = "user_123",
            ["role"] = "admin"
        };

        // Act
        var result = VKPromptXmlBuilder.WrapSelfClosing("user", attrs);

        // Assert
        result.Should().Be("<user id=\"user_123\" role=\"admin\" />");
    }

    [Fact]
    public void WrapSelfClosing_WhenInvalidArguments_ThrowsException()
    {
        Assert.ThrowsAny<ArgumentException>(() => VKPromptXmlBuilder.WrapSelfClosing(null!, new Dictionary<string, string>()));
        Assert.Throws<ArgumentException>(() => VKPromptXmlBuilder.WrapSelfClosing("", new Dictionary<string, string>()));
        Assert.Throws<ArgumentNullException>(() => VKPromptXmlBuilder.WrapSelfClosing("tag", null!));
    }

    [Fact]
    public void Wrap_SingleContentAlreadyWrappedWithSameTag_DoesNotDoubleWrap()
    {
        // Arrange
        var content = "<instruction>\r\nAlready wrapped.\r\n</instruction>";

        // Act
        var result = VKPromptXmlBuilder.Wrap("instruction", content);

        // Assert
        result.Should().Be(content);
    }

    [Fact]
    public void Wrap_MultipleItemsWithSingleAlreadyWrapped_DoesNotDoubleWrap()
    {
        // Arrange
        var items = new[] { "<rules>\nAlready wrapped rules\n</rules>" };

        // Act
        var result = VKPromptXmlBuilder.Wrap("rules", items);

        // Assert
        result.Should().Be("<rules>\nAlready wrapped rules\n</rules>");
    }

    [Fact]
    public void Wrap_SingleContentAlreadyWrappedWithDifferentTag_EnclosesInNewTag()
    {
        // Arrange
        var content = "<inner>Text</inner>";

        // Act
        var result = VKPromptXmlBuilder.Wrap("outer", content);

        // Assert
        result.Should().StartWith("<outer>");
        result.Should().Contain("<inner>Text</inner>");
        result.Should().EndWith("</outer>");
    }

    [Fact]
    public void Wrap_SingleContentWithSiblingBlocksOfSameTag_CoalescesIntoSingleRootTag()
    {
        // Arrange
        var content = "<system_directive>\n123\n</system_directive>\n<system_directive>\n456\n</system_directive>";

        // Act
        var result = VKPromptXmlBuilder.Wrap("system_directive", content);

        // Assert
        result.Should().StartWith("<system_directive>\r\n");
        result.Should().Contain("123");
        result.Should().Contain("456");
        result.Should().EndWith("</system_directive>");
        result.IndexOf("<system_directive>").Should().Be(result.LastIndexOf("<system_directive>"));
        result.IndexOf("</system_directive>").Should().Be(result.LastIndexOf("</system_directive>"));
    }

    [Fact]
    public void Wrap_MultipleItemsEachAlreadyWrapped_UnwrapsAndCoalescesIntoSingleRootTag()
    {
        // Arrange
        var items = new[]
        {
            "<system_directive>\n123\n</system_directive>",
            "<system_directive>\n456\n</system_directive>"
        };

        // Act
        var result = VKPromptXmlBuilder.Wrap("system_directive", items);

        // Assert
        result.Should().StartWith("<system_directive>\r\n");
        result.Should().Contain("123");
        result.Should().Contain("456");
        result.Should().EndWith("</system_directive>");
        result.IndexOf("<system_directive>").Should().Be(result.LastIndexOf("<system_directive>"));
        result.IndexOf("</system_directive>").Should().Be(result.LastIndexOf("</system_directive>"));
    }

    [Fact]
    public void Wrap_MultipleItemsMixedWrapping_NormalizesWithoutDoubleWrapping()
    {
        // Arrange
        var items = new[]
        {
            "<rules>\nRule 1\n</rules>",
            "Rule 2",
            "<persona>Persona P</persona>"
        };

        // Act
        var result = VKPromptXmlBuilder.Wrap("rules", items);

        // Assert
        result.Should().StartWith("<rules>\r\n");
        result.Should().Contain("Rule 1");
        result.Should().Contain("Rule 2");
        result.Should().Contain("<persona>Persona P</persona>");
        result.Should().EndWith("</rules>");
        result.IndexOf("<rules>").Should().Be(result.LastIndexOf("<rules>"));
        result.IndexOf("</rules>").Should().Be(result.LastIndexOf("</rules>"));
    }

    [Fact]
    public void Unwrap_SingleWrappedBlock_ReturnsInnerContent()
    {
        // Arrange
        var content = "<system_directive>\n123\n</system_directive>";

        // Act
        var result = VKPromptXmlBuilder.Unwrap("system_directive", content);

        // Assert
        result.Should().Be("123");
    }

    [Fact]
    public void Unwrap_SiblingBlocks_ReturnsJoinedInnerContents()
    {
        // Arrange
        var content = "<system_directive>\n123\n</system_directive>\n<system_directive>\n456\n</system_directive>";

        // Act
        var result = VKPromptXmlBuilder.Unwrap("system_directive", content);

        // Assert
        result.Should().Be("123\n\n456");
    }

    [Fact]
    public void Unwrap_NotWrapped_ReturnsOriginalContent()
    {
        // Arrange
        var content = "Plain text content";

        // Act
        var result = VKPromptXmlBuilder.Unwrap("system_directive", content);

        // Assert
        result.Should().Be("Plain text content");
    }
}
