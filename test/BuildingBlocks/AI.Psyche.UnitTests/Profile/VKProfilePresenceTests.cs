using VK.Blocks.AI.Psyche.UnitTests.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Profile;

/// <summary>
/// Unit tests for <see cref="VKProfilePresence"/> aggregate root.
/// Follows AP.01, CS.01, and DL.01 rules.
/// </summary>
public sealed class VKProfilePresenceTests : VKUnitTestBase
{
    [Fact]
    public void Create_WithValidParameters_ReturnsSuccess()
    {
        // Arrange
        var id = new VKProfileId(Guid.NewGuid());

        // Act
        var result = VKProfilePresence.Create(id, "Alice", "en-US", "UTC", "User prefers code-first answers");

        // Assert
        result.Should().BeSuccess();
        var profile = result.Value!;
        profile.Id.Should().Be(id);
        profile.DisplayName.Should().Be("Alice");
        profile.PreferredLanguage.Should().Be("en-US");
        profile.TimeZone.Should().Be("UTC");
        profile.Description.Should().Be("User prefers code-first answers");
        profile.TagName.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyId_ThrowsException()
    {
        // Act
        Action act = () => VKProfilePresence.Create(VKProfileId.Empty);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Rehydrate_WithValidParameters_RestoresAggregate()
    {
        // Arrange
        var id = new VKProfileId(Guid.NewGuid());

        // Act
        var profile = VKProfilePresence.Rehydrate(id, "Bob", "ja-JP", "Asia/Tokyo", "Japanese native speaker", VKPromptRelativeDepth.AfterDirective, 10, null, "profile", 20);

        // Assert
        profile.Id.Should().Be(id);
        profile.DisplayName.Should().Be("Bob");
        profile.PreferredLanguage.Should().Be("ja-JP");
        profile.TimeZone.Should().Be("Asia/Tokyo");
        profile.Description.Should().Be("Japanese native speaker");
        profile.RelativeDepth.Should().Be(VKPromptRelativeDepth.AfterDirective);
        profile.DepthPriority.Should().Be(10);
        profile.TokenCount.Should().Be(20);
    }

    [Fact]
    public void UpdateSettings_WhenCalled_UpdatesSettings()
    {
        // Arrange
        var profile = new VKProfilePresenceBuilder().Build();

        // Act
        var result = profile.UpdateSettings("Charlie", "fr-FR", "Europe/Paris");

        // Assert
        result.Should().BeSuccess();
        profile.DisplayName.Should().Be("Charlie");
        profile.PreferredLanguage.Should().Be("fr-FR");
        profile.TimeZone.Should().Be("Europe/Paris");
    }

    [Fact]
    public void UpdateDescription_WhenCalled_UpdatesDescription()
    {
        // Arrange
        var profile = new VKProfilePresenceBuilder().Build();

        // Act
        var result = profile.UpdateDescription("Updated bio");

        // Assert
        result.Should().BeSuccess();
        profile.Description.Should().Be("Updated bio");
    }

    [Fact]
    public void UpdateTokenCount_WhenCalled_UpdatesTokenCount()
    {
        // Arrange
        var profile = new VKProfilePresenceBuilder().Build();

        // Act
        var result = profile.UpdateTokenCount(42);

        // Assert
        result.Should().BeSuccess();
        profile.TokenCount.Should().Be(42);
    }
}
