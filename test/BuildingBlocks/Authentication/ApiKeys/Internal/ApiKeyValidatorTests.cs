using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VK.Blocks.Authentication.ApiKeys.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.UnitTests.ApiKeys.Internal;

public sealed class ApiKeyValidatorTests
{
    private readonly Mock<IVKApiKeyStore> _storeMock;
    private readonly Mock<IVKApiKeyRevocationProvider> _revocationProviderMock;
    private readonly Mock<IVKApiKeyRateLimiter> _rateLimiterMock;
    private readonly Mock<IOptions<VKApiKeyOptions>> _optionsMock;
    private readonly Mock<TimeProvider> _timeProviderMock;
    private readonly Mock<ILogger<ApiKeyValidator>> _loggerMock;
    private readonly ApiKeyValidator _sut;
    private readonly Fixture _fixture;
    private readonly VKApiKeyOptions _options;

    public ApiKeyValidatorTests()
    {
        _storeMock = new Mock<IVKApiKeyStore>();
        _revocationProviderMock = new Mock<IVKApiKeyRevocationProvider>();
        _rateLimiterMock = new Mock<IVKApiKeyRateLimiter>();
        _optionsMock = new Mock<IOptions<VKApiKeyOptions>>();
        _timeProviderMock = new Mock<TimeProvider>();
        _loggerMock = new Mock<ILogger<ApiKeyValidator>>();
        _fixture = new Fixture();

        _options = new VKApiKeyOptions
        {
            Enabled = true,
            MinLength = 32,
            EnableRateLimiting = true,
            RateLimitPerMinute = 60,
            RateLimitWindowSeconds = 60,
            TrackLastUsedAt = true
        };

        _optionsMock.Setup(x => x.Value).Returns(_options);
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(DateTimeOffset.UtcNow);

        _revocationProviderMock.Setup(x => x.IsRevokedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _rateLimiterMock.Setup(x => x.IsAllowedAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _sut = new ApiKeyValidator(
            _storeMock.Object,
            _revocationProviderMock.Object,
            _rateLimiterMock.Object,
            _optionsMock.Object,
            _timeProviderMock.Object,
            _loggerMock.Object);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateAsync_InvalidInput_ReturnsInvalidError(string? rawApiKey)
    {
        // Arrange
        // No additional setup needed

        // Act
        var result = await _sut.ValidateAsync(rawApiKey!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(VKApiKeyErrors.Invalid);
    }

    [Fact]
    public async Task ValidateAsync_TooShortKey_ReturnsInvalidError()
    {
        // Arrange
        var shortKey = new string('a', _options.MinLength - 1);

        // Act
        var result = await _sut.ValidateAsync(shortKey);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(VKApiKeyErrors.Invalid);
    }

    [Fact]
    public async Task ValidateAsync_KeyNotFound_ReturnsInvalidError()
    {
        // Arrange
        var rawKey = _fixture.Create<string>().PadRight(_options.MinLength, 'a');
        _storeMock.Setup(x => x.FindByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Failure<VKApiKeyRecord>(VKApiKeyErrors.Invalid));

        // Act
        var result = await _sut.ValidateAsync(rawKey);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(VKApiKeyErrors.Invalid);
    }

    [Fact]
    public async Task ValidateAsync_KeyRevoked_ReturnsRevokedError()
    {
        // Arrange
        var rawKey = _fixture.Create<string>().PadRight(_options.MinLength, 'a');
        var record = _fixture.Build<VKApiKeyRecord>()
            .With(x => x.IsEnabled, true)
            .With(x => x.ExpiresAt, (DateTimeOffset?)null)
            .Create();

        _storeMock.Setup(x => x.FindByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(record));

        _revocationProviderMock.Setup(x => x.IsRevokedAsync(record.Id.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.ValidateAsync(rawKey);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(VKApiKeyErrors.Revoked);
    }

    [Fact]
    public async Task ValidateAsync_KeyExpired_ReturnsExpiredError()
    {
        // Arrange
        var rawKey = _fixture.Create<string>().PadRight(_options.MinLength, 'a');
        var now = DateTimeOffset.UtcNow;
        var record = _fixture.Build<VKApiKeyRecord>()
            .With(x => x.IsEnabled, true)
            .With(x => x.ExpiresAt, now.AddMinutes(-10))
            .Create();

        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(now);
        _storeMock.Setup(x => x.FindByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(record));
        _revocationProviderMock.Setup(x => x.IsRevokedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.ValidateAsync(rawKey);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(VKApiKeyErrors.Expired);
    }

    [Fact]
    public async Task ValidateAsync_KeyDisabled_ReturnsDisabledError()
    {
        // Arrange
        var rawKey = _fixture.Create<string>().PadRight(_options.MinLength, 'a');
        var record = _fixture.Build<VKApiKeyRecord>()
            .With(x => x.IsEnabled, false)
            .With(x => x.ExpiresAt, (DateTimeOffset?)null)
            .Create();

        _storeMock.Setup(x => x.FindByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(record));
        _revocationProviderMock.Setup(x => x.IsRevokedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.ValidateAsync(rawKey);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(VKApiKeyErrors.Disabled);
    }

    [Fact]
    public async Task ValidateAsync_RateLimitExceeded_ReturnsRateLimitExceededError()
    {
        // Arrange
        var rawKey = _fixture.Create<string>().PadRight(_options.MinLength, 'a');
        var record = _fixture.Build<VKApiKeyRecord>()
            .With(x => x.IsEnabled, true)
            .With(x => x.ExpiresAt, (DateTimeOffset?)null)
            .Create();

        _storeMock.Setup(x => x.FindByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(record));
        _revocationProviderMock.Setup(x => x.IsRevokedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _rateLimiterMock.Setup(x => x.IsAllowedAsync(record.Id, _options.RateLimitPerMinute, _options.RateLimitWindowSeconds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.ValidateAsync(rawKey);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(VKApiKeyErrors.RateLimitExceeded);
    }

    [Fact]
    public async Task ValidateAsync_ValidKey_ReturnsSuccess()
    {
        // Arrange
        var rawKey = _fixture.Create<string>().PadRight(_options.MinLength, 'a');
        var record = _fixture.Build<VKApiKeyRecord>()
            .With(x => x.IsEnabled, true)
            .With(x => x.ExpiresAt, (DateTimeOffset?)null)
            .Create();

        _storeMock.Setup(x => x.FindByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(record));
        _revocationProviderMock.Setup(x => x.IsRevokedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _rateLimiterMock.Setup(x => x.IsAllowedAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.ValidateAsync(rawKey);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.KeyId.Should().Be(record.Id);
        _storeMock.Verify(x => x.UpdateLastUsedAtAsync(record.Id, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateAsync_UpdateLastUsedFails_StillReturnsSuccess()
    {
        // Arrange
        var rawKey = _fixture.Create<string>().PadRight(_options.MinLength, 'a');
        var record = _fixture.Build<VKApiKeyRecord>()
            .With(x => x.IsEnabled, true)
            .With(x => x.ExpiresAt, (DateTimeOffset?)null)
            .Create();

        _storeMock.Setup(x => x.FindByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(record));
        _revocationProviderMock.Setup(x => x.IsRevokedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _rateLimiterMock.Setup(x => x.IsAllowedAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _storeMock.Setup(x => x.UpdateLastUsedAtAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _sut.ValidateAsync(rawKey);

        // Assert
        result.IsSuccess.Should().BeTrue("Validation should succeed even if last-used update fails");
        result.Value!.KeyId.Should().Be(record.Id);
    }

    [Fact]
    public async Task ValidateAsync_TrackLastUsedAtDisabled_DoesNotUpdate()
    {
        // Arrange
        var options = _options with { TrackLastUsedAt = false };
        _optionsMock.Setup(x => x.Value).Returns(options);
        var rawKey = _fixture.Create<string>().PadRight(_options.MinLength, 'a');
        var record = _fixture.Build<VKApiKeyRecord>()
            .With(x => x.IsEnabled, true)
            .With(x => x.ExpiresAt, (DateTimeOffset?)null)
            .Create();

        _storeMock.Setup(x => x.FindByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(record));
        _revocationProviderMock.Setup(x => x.IsRevokedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _sut.ValidateAsync(rawKey);

        // Assert
        _storeMock.Verify(x => x.UpdateLastUsedAtAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_RateLimitingDisabled_DoesNotCheckRateLimiter()
    {
        // Arrange
        var options = _options with { EnableRateLimiting = false };
        _optionsMock.Setup(x => x.Value).Returns(options);
        var rawKey = _fixture.Create<string>().PadRight(_options.MinLength, 'a');
        var record = _fixture.Build<VKApiKeyRecord>()
            .With(x => x.IsEnabled, true)
            .With(x => x.ExpiresAt, (DateTimeOffset?)null)
            .Create();

        _storeMock.Setup(x => x.FindByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(record));
        _revocationProviderMock.Setup(x => x.IsRevokedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _sut.ValidateAsync(rawKey);

        // Assert
        _rateLimiterMock.Verify(x => x.IsAllowedAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_LargeKey_HashesCorrectly()
    {
        // Arrange
        // Key longer than 256 bytes to trigger ArrayPool logic
        var rawKey = new string('a', 300);
        var record = _fixture.Build<VKApiKeyRecord>()
            .With(x => x.IsEnabled, true)
            .With(x => x.ExpiresAt, (DateTimeOffset?)null)
            .Create();

        _storeMock.Setup(x => x.FindByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(record));
        _revocationProviderMock.Setup(x => x.IsRevokedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.ValidateAsync(rawKey);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
