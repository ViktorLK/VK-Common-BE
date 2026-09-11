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
}
