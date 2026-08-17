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
    public async Task GetProfileAsync_WhenSeededAndTenantMatches_ReturnsPresence()
    {
        // Arrange
        var identityMock = new Mock<IVKIdentityContext>();
        identityMock.SetupGet(i => i.TenantId).Returns(VKTenantId.Default);

        var store = new InMemoryProfileStore(identityMock.Object);
        var userId = new VKUserId(Guid.NewGuid());
        var profile = new VKProfilePresence
        {
            UserId = userId,
            TenantId = VKTenantId.Default,
            PreferredLanguage = "ja-JP"
        };
        store.Seed(profile);

        // Act
        var result = await store.GetProfileAsync(userId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(profile);
    }

    [Fact]
    public async Task SaveProfileAsync_SavesProfileSuccessfully()
    {
        // Arrange
        var identityMock = new Mock<IVKIdentityContext>();
        identityMock.SetupGet(i => i.TenantId).Returns(VKTenantId.Default);

        var store = new InMemoryProfileStore(identityMock.Object);
        var userId = new VKUserId(Guid.NewGuid());
        var profile = new VKProfilePresence
        {
            UserId = userId,
            TenantId = VKTenantId.Default
        };

        // Act
        var saveRes = await store.SaveProfileAsync(profile, CancellationToken.None);
        var getRes = await store.GetProfileAsync(userId, CancellationToken.None);

        // Assert
        saveRes.IsSuccess.Should().BeTrue();
        getRes.Value.Should().Be(profile);
    }
}
