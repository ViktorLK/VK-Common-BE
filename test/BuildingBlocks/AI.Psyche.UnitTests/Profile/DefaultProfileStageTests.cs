using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
            .WithContent("Custom user bio information")
            .Build();

        GetMock<IVKPsycheProfileRepository>()
            .Setup(s => s.FindByIdAsync(profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(profile));

        var options = new VKProfileOptions { Enabled = true };
        var stage = new DefaultProfileStage(
            options,
            GetMockObject<IVKPsycheProfileRepository>(),
            new DefaultProfileRenderer(TimeProvider.System),
            GetMockObject<ILogger<DefaultProfileStage>>());

        var (context, _) = new VKPsycheRequestBuilder()
            .WithProfileId(profile.Id)
            .WithUserInput("hello")
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.State<VKProfilePresence>().Should().Be(profile);
        var segment = context.Segments.Should().ContainSingle(s => s.Tier == VKPromptTierType.Profile).Subject;
        segment.Role.Should().Be(VKChatRole.System);
        segment.Content.Should().Contain("zh-CN");
        segment.Content.Should().Contain("UTC");
        segment.Content.Should().Contain("Custom user bio information");
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoProfileId_ReturnsSuccessWithoutFragments()
    {
        // Arrange
        var options = new VKProfileOptions { Enabled = true };
        var stage = new DefaultProfileStage(
            options,
            GetMockObject<IVKPsycheProfileRepository>(),
            GetMockObject<IVKProfileRenderer>(),
            GetMockObject<ILogger<DefaultProfileStage>>());

        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hello").BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.Segments.Should().BeEmpty();
    }
}
