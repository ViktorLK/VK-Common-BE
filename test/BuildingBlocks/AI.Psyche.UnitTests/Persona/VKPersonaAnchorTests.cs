using VK.Blocks.AI.Psyche.UnitTests.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Persona;

/// <summary>
/// Unit tests for <see cref="VKPersonaAnchor"/> aggregate root.
/// Follows AP.01, CS.01, and DL.01 rules.
/// </summary>
public sealed class VKPersonaAnchorTests : VKUnitTestBase
{
    [Fact]
    public void Create_WithValidParameters_ReturnsSuccess()
    {
        // Arrange
        var id = new VKPersonaId(Guid.NewGuid());
        var traits = new Dictionary<string, string> { ["Tone"] = "Calm" };
        var extensions = new Dictionary<string, object> { ["Version"] = 1 };

        // Act
        var result = VKPersonaAnchor.Create(id, "Aegis", "Guardian AI", traits, extensions);

        // Assert
        result.Should().BeSuccess();
        var persona = result.Value!;
        persona.Id.Should().Be(id);
        persona.Name.Should().Be("Aegis");
        persona.Description.Should().Be("Guardian AI");
        persona.Traits.Should().ContainKey("Tone").WhoseValue.Should().Be("Calm");
        persona.Extensions.Should().ContainKey("Version");
    }

    [Fact]
    public void Create_WithEmptyId_ThrowsException()
    {
        // Act
        Action act = () => VKPersonaAnchor.Create(VKPersonaId.Empty, "Name", "Desc");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ThrowsException(string invalidName)
    {
        // Act
        Action act = () => VKPersonaAnchor.Create(new VKPersonaId(Guid.NewGuid()), invalidName, "Desc");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Rehydrate_WithValidParameters_RestoresAggregate()
    {
        // Arrange
        var id = new VKPersonaId(Guid.NewGuid());
        var traits = new Dictionary<string, string> { ["Mood"] = "Happy" };

        // Act
        var persona = VKPersonaAnchor.Rehydrate(id, "Aegis", "Desc", traits);

        // Assert
        persona.Id.Should().Be(id);
        persona.Name.Should().Be("Aegis");
        persona.Traits.Should().ContainKey("Mood");
    }

    [Fact]
    public void UpdateDetails_WhenCalled_UpdatesNameAndDescription()
    {
        // Arrange
        var persona = new VKPersonaAnchorBuilder().WithName("Old").WithDescription("Old Desc").Build();

        // Act
        var result = persona.UpdateDetails("New Name", "New Desc");

        // Assert
        result.Should().BeSuccess();
        persona.Name.Should().Be("New Name");
        persona.Description.Should().Be("New Desc");
    }

    [Fact]
    public void SetTrait_WhenCalled_AddsOrUpdatesTrait()
    {
        // Arrange
        var persona = new VKPersonaAnchorBuilder().Build();

        // Act
        persona.SetTrait("Role", "Advisor");
        persona.SetTrait("Role", "Leader");

        // Assert
        persona.Traits.Should().ContainKey("Role").WhoseValue.Should().Be("Leader");
    }

    [Fact]
    public void RemoveTrait_WhenTraitExists_RemovesTrait()
    {
        // Arrange
        var persona = new VKPersonaAnchorBuilder()
            .WithTrait("Role", "Advisor")
            .Build();

        // Act
        persona.RemoveTrait("Role");
        persona.RemoveTrait("NonExistent");

        // Assert
        persona.Traits.Should().NotContainKey("Role");
    }

    [Fact]
    public void ReplaceTraits_WhenCalled_ReplacesAllTraits()
    {
        // Arrange
        var persona = new VKPersonaAnchorBuilder()
            .WithTraits(new Dictionary<string, string> { ["A"] = "1" })
            .Build();

        // Act
        persona.ReplaceTraits(new Dictionary<string, string> { ["B"] = "2", ["C"] = "3" });

        // Assert
        persona.Traits.Should().NotContainKey("A");
        persona.Traits.Should().HaveCount(2);
    }

    [Fact]
    public void SetExtension_WhenCalled_SetsExtension()
    {
        // Arrange
        var persona = new VKPersonaAnchorBuilder().Build();

        // Act
        persona.SetExtension("key1", "val1");

        // Assert
        persona.Extensions.Should().ContainKey("key1").WhoseValue.Should().Be("val1");
    }
}
