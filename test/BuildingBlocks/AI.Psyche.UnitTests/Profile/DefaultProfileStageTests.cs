using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VK.Blocks.AI.Psyche.Profile.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Profile;

public sealed class DefaultProfileStageTests
{
    [Fact]
    public async Task ExecuteAsync_WithProfileInStore_InjectsPreferredLanguageAndTimeZoneFragments()
    {
        // Arrange
        var storeMock = new Mock<IVKProfileStore>();
        var profileId = new VKProfileId(Guid.NewGuid());

        var profile = new VKProfilePresence
        {
            Id = profileId,
            PreferredLanguage = "zh-CN",
            TimeZone = "UTC",
            Preferences = new Dictionary<string, string> { ["Format"] = "Markdown" }
        };
        storeMock.Setup(s => s.GetProfileAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<VKProfilePresence?>(profile));

        var options = new VKProfileOptions { Enabled = true };
        var stage = new DefaultProfileStage(options, storeMock.Object, TimeProvider.System);

        var (context, _) = new VKPsycheRequestBuilder()
            .WithProfileId(profileId)
            .WithUserInput("hello")
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        context.State<VKProfilePresence>().Should().Be(profile);
        context.Fragments.Should().Contain(f => f.Segment.Content.Contains("zh-CN"));
        context.Fragments.Should().Contain(f => f.Segment.Content.Contains("UTC"));
        context.Fragments.Should().Contain(f => f.Segment.Content.Contains("Format: Markdown"));
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoProfileId_ReturnsSuccessWithoutFragments()
    {
        // Arrange
        var storeMock = new Mock<IVKProfileStore>();
        var options = new VKProfileOptions { Enabled = true };
        var stage = new DefaultProfileStage(options, storeMock.Object);

        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hello").BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        context.Fragments.Should().BeEmpty();
    }
}
