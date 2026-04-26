using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VK.Blocks.Authentication.Jwt.Internal;

using VK.Blocks.Authentication.UnitTests.Common;

namespace VK.Blocks.Authentication.UnitTests.Jwt.Internal;

public sealed class InMemoryJwtRefreshTokenValidatorTests
{
    private readonly Mock<IOptions<VKJwtOptions>> _optionsMock;
    private readonly FakeTimeProvider _timeProvider;
    private readonly Mock<ILogger<InMemoryJwtRefreshTokenValidator>> _loggerMock;
    private readonly InMemoryJwtRefreshTokenValidator _validator;

    public InMemoryJwtRefreshTokenValidatorTests()
    {
        _optionsMock = new Mock<IOptions<VKJwtOptions>>();
        _optionsMock.Setup(x => x.Value).Returns(new VKJwtOptions { RefreshTokenLifetimeDays = 7 });
        _timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        _loggerMock = new Mock<ILogger<InMemoryJwtRefreshTokenValidator>>();
        _validator = new InMemoryJwtRefreshTokenValidator(_optionsMock.Object, _timeProvider, _loggerMock.Object);
    }

    [Fact]
    public async Task ValidateTokenRotationAsync_WithValidNewJti_ShouldReturnSuccess()
    {
        // Act
        var result = await _validator.ValidateTokenRotationAsync("new-jti", "family-1");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateTokenRotationAsync_WithReplayedJti_ShouldReturnFailure()
    {
        // Arrange
        await _validator.ValidateTokenRotationAsync("replay-jti", "family-1");

        // Act
        var result = await _validator.ValidateTokenRotationAsync("replay-jti", "family-1");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(JwtRefreshTokenErrors.Compromised);
    }

    [Fact]
    public async Task ValidateTokenRotationAsync_WithExpiredCacheJti_ShouldReturnSuccess()
    {
        // Arrange
        _optionsMock.Setup(x => x.Value).Returns(new VKJwtOptions { RefreshTokenLifetimeDays = 1 });
        await _validator.ValidateTokenRotationAsync("expired-jti", "family-1");

        // Move time past expiration
        _timeProvider.Advance(TimeSpan.FromDays(2));

        // Act
        var result = await _validator.ValidateTokenRotationAsync("expired-jti", "family-1");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "family-1")]
    [InlineData("jti-1", "")]
    [InlineData(null, "family-1")]
    [InlineData("jti-1", null)]
    public async Task ValidateTokenRotationAsync_WithInvalidIds_ShouldReturnFailure(string? jti, string? familyId)
    {
        // Act
        var result = await _validator.ValidateTokenRotationAsync(jti!, familyId!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(JwtRefreshTokenErrors.InvalidIds);
    }

    [Fact]
    public async Task CleanupExpiredEntries_ShouldRemoveExpiredKeys()
    {
        // Arrange
        _optionsMock.Setup(x => x.Value).Returns(new VKJwtOptions { RefreshTokenLifetimeDays = 1 });
        await _validator.ValidateTokenRotationAsync("jti-to-expire", "fam-1");
        await _validator.ValidateTokenRotationAsync("jti-to-keep", "fam-2");

        // Advance time for jti-to-expire
        _timeProvider.Advance(TimeSpan.FromDays(2));

        // Act
        _validator.CleanupExpiredEntries();

        // Assert
        // If we try to validate the expired one again, it should be success because it's no longer in the dictionary
        var result = await _validator.ValidateTokenRotationAsync("jti-to-expire", "fam-1");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void AssociatedServiceType_ShouldBeCorrect()
    {
        // Assert
        _validator.AssociatedServiceType.Should().Be<IVKJwtRefreshValidator>();
    }
}
