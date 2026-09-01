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
        var prefs = new Dictionary<string, string> { ["Theme"] = "Dark" };

        // Act
        var result = VKProfilePresence.Create(id, "Alice", "en-US", "UTC", prefs);

        // Assert
        result.Should().BeSuccess();
        var profile = result.Value!;
        profile.Id.Should().Be(id);
        profile.DisplayName.Should().Be("Alice");
        profile.PreferredLanguage.Should().Be("en-US");
        profile.TimeZone.Should().Be("UTC");
        profile.Preferences.Should().ContainKey("Theme");
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
        var prefs = new Dictionary<string, string> { ["Style"] = "Compact" };

        // Act
        var profile = VKProfilePresence.Rehydrate(id, "Bob", "ja-JP", "Asia/Tokyo", prefs);

        // Assert
        profile.Id.Should().Be(id);
        profile.DisplayName.Should().Be("Bob");
        profile.PreferredLanguage.Should().Be("ja-JP");
        profile.TimeZone.Should().Be("Asia/Tokyo");
        profile.Preferences.Should().ContainKey("Style");
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
    public void SetPreference_WhenCalled_AddsOrUpdatesPreference()
    {
        // Arrange
        var profile = new VKProfilePresenceBuilder().Build();

        // Act
        profile.SetPreference("Format", "Detailed");
        profile.SetPreference("Format", "Concise");

        // Assert
        profile.Preferences.Should().ContainKey("Format").WhoseValue.Should().Be("Concise");
    }

    [Fact]
    public void RemovePreference_WhenPreferenceExists_RemovesIt()
    {
        // Arrange
        var profile = new VKProfilePresenceBuilder()
            .WithPreference("Key1", "Val1")
            .Build();

        // Act
        profile.RemovePreference("Key1");
        profile.RemovePreference("NonExistent");

        // Assert
        profile.Preferences.Should().NotContainKey("Key1");
    }
}
