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
        var identityMock = new Mock<IVKIdentityContext>();
        var userId = new VKUserId(Guid.NewGuid());
        identityMock.SetupGet(i => i.UserId).Returns(userId);

        var profile = new VKProfilePresence
        {
            UserId = userId,
            TenantId = VKTenantId.Default,
            PreferredLanguage = "zh-CN",
            TimeZone = "UTC",
            Preferences = new Dictionary<string, string> { ["Format"] = "Markdown" }
        };
        storeMock.Setup(s => s.GetProfileAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<VKProfilePresence?>(profile));

        var options = new VKProfileOptions { Enabled = true };
        var stage = new DefaultProfileStage(options, storeMock.Object, identityMock.Object, TimeProvider.System);

        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hello").BuildContext();

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
    public async Task ExecuteAsync_WhenNoUserId_ReturnsSuccessWithoutFragments()
    {
        // Arrange
        var storeMock = new Mock<IVKProfileStore>();
        var identityMock = new Mock<IVKIdentityContext>();
        identityMock.SetupGet(i => i.UserId).Returns(VKUserId.Empty);

        var options = new VKProfileOptions { Enabled = true };
        var stage = new DefaultProfileStage(options, storeMock.Object, identityMock.Object);

        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hello").BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        context.Fragments.Should().BeEmpty();
    }
}
