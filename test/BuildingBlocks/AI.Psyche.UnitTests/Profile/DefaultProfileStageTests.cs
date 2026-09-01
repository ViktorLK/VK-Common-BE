using Moq;
using VK.Blocks.AI.Psyche.Profile.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Profile;

public sealed class DefaultProfileStageTests : VKUnitTestBase
{
    [Fact]
    public async Task ExecuteAsync_WithProfileInStore_InjectsPreferredLanguageAndTimeZoneFragments()
    {
        // Arrange
        var profile = new VKProfilePresenceBuilder()
            .WithPreferredLanguage("zh-CN")
            .WithTimeZone("UTC")
            .WithPreference("Format", "Markdown")
            .Build();

        GetMock<IVKPsycheProfileRepository>()
            .Setup(s => s.FindByIdAsync(profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(profile));

        var options = new VKProfileOptions { Enabled = true };
        var stage = new DefaultProfileStage(options, GetMockObject<IVKPsycheProfileRepository>(), TimeProvider.System);

        var (context, _) = new VKPsycheRequestBuilder()
            .WithProfileId(profile.Id)
            .WithUserInput("hello")
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.State<VKProfilePresence>().Should().Be(profile);
        context.Fragments.Should().Contain(f => f.Segment.Content.Contains("zh-CN"));
        context.Fragments.Should().Contain(f => f.Segment.Content.Contains("UTC"));
        context.Fragments.Should().Contain(f => f.Segment.Content.Contains("Format: Markdown"));
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoProfileId_ReturnsSuccessWithoutFragments()
    {
        // Arrange
        var options = new VKProfileOptions { Enabled = true };
        var stage = new DefaultProfileStage(options, GetMockObject<IVKPsycheProfileRepository>());

        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hello").BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.Fragments.Should().BeEmpty();
    }
}
