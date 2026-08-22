using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VK.Blocks.AI.Psyche.Profile.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Profile;

public sealed class InMemoryProfileStoreTests
{
    [Fact]
    public async Task GetProfileAsync_WhenSeeded_ReturnsPresence()
    {
        // Arrange
        var store = new InMemoryProfileStore();
        var profileId = new VKProfileId(Guid.NewGuid());
        var profile = new VKProfilePresence
        {
            Id = profileId,
            PreferredLanguage = "ja-JP"
        };
        store.Seed(profile);

        // Act
        var result = await store.GetProfileAsync(profileId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(profile);
    }

    [Fact]
    public async Task GetProfileAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var store = new InMemoryProfileStore();
        var profileId = new VKProfileId(Guid.NewGuid());

        // Act
        var getRes = await store.GetProfileAsync(profileId, CancellationToken.None);

        // Assert
        getRes.IsSuccess.Should().BeTrue();
        Action act = () => _ = getRes.Value;
        act.Should().Throw<InvalidOperationException>();
    }
}
