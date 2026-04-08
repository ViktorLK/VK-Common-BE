using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VK.Blocks.Authentication.Features.ApiKeys;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authentication.UnitTests.Features.ApiKeys;

public sealed class ApiKeyValidatorTests
{
    private readonly Mock<IApiKeyStore> _storeMock;
    private readonly Mock<IApiKeyRevocationProvider> _revocationProviderMock;
    private readonly Mock<IApiKeyRateLimiter> _rateLimiterMock;
    private readonly Mock<IOptionsMonitor<ApiKeyOptions>> _optionsMonitorMock;
    private readonly Mock<TimeProvider> _timeProviderMock;
    private readonly Mock<ILogger<ApiKeyValidator>> _loggerMock;
    private readonly ApiKeyValidator _sut;
    private readonly Fixture _fixture;
    private readonly ApiKeyOptions _options;

    public ApiKeyValidatorTests()
    {
        _storeMock = new Mock<IApiKeyStore>();
        _revocationProviderMock = new Mock<IApiKeyRevocationProvider>();
        _rateLimiterMock = new Mock<IApiKeyRateLimiter>();
        _optionsMonitorMock = new Mock<IOptionsMonitor<ApiKeyOptions>>();
        _timeProviderMock = new Mock<TimeProvider>();
        _loggerMock = new Mock<ILogger<ApiKeyValidator>>();
        _fixture = new Fixture();

        _options = new ApiKeyOptions
        {
            Enabled = true,
            MinLength = 32,
            EnableRateLimiting = true,
            RateLimitPerMinute = 60,
            RateLimitWindowSeconds = 60,
            TrackLastUsedAt = true
        };

        _optionsMonitorMock.Setup(x => x.CurrentValue).Returns(_options);
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(DateTimeOffset.UtcNow);

        _sut = new ApiKeyValidator(
            _storeMock.Object,
            _revocationProviderMock.Object,
            _rateLimiterMock.Object,
            _optionsMonitorMock.Object,
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
        result.Errors.Should().Contain(ApiKeyErrors.Invalid);
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
        result.Errors.Should().Contain(ApiKeyErrors.Invalid);
    }

    [Fact]
    public async Task ValidateAsync_KeyNotFound_ReturnsInvalidError()
    {
        // Arrange
        var rawKey = _fixture.Create<string>().PadRight(_options.MinLength, 'a');
        _storeMock.Setup(x => x.FindByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<ApiKeyRecord>(ApiKeyErrors.Invalid));

        // Act
        var result = await _sut.ValidateAsync(rawKey);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(ApiKeyErrors.Invalid);
    }

    [Fact]
    public async Task ValidateAsync_KeyRevoked_ReturnsRevokedError()
    {
        // Arrange
        var rawKey = _fixture.Create<string>().PadRight(_options.MinLength, 'a');
        var record = _fixture.Build<ApiKeyRecord>()
            .With(x => x.IsEnabled, true)
            .With(x => x.ExpiresAt, (DateTimeOffset?)null)
            .Create();

        _storeMock.Setup(x => x.FindByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(record));

        _revocationProviderMock.Setup(x => x.IsRevokedAsync(record.Id.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.ValidateAsync(rawKey);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(ApiKeyErrors.Revoked);
    }

    [Fact]
    public async Task ValidateAsync_KeyExpired_ReturnsExpiredError()
    {
        // Arrange
        var rawKey = _fixture.Create<string>().PadRight(_options.MinLength, 'a');
        var now = DateTimeOffset.UtcNow;
        var record = _fixture.Build<ApiKeyRecord>()
            .With(x => x.IsEnabled, true)
            .With(x => x.ExpiresAt, now.AddMinutes(-10))
            .Create();

        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(now);
        _storeMock.Setup(x => x.FindByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(record));
        _revocationProviderMock.Setup(x => x.IsRevokedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.ValidateAsync(rawKey);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(ApiKeyErrors.Expired);
    }

    [Fact]
    public async Task ValidateAsync_KeyDisabled_ReturnsDisabledError()
    {
        // Arrange
        var rawKey = _fixture.Create<string>().PadRight(_options.MinLength, 'a');
        var record = _fixture.Build<ApiKeyRecord>()
            .With(x => x.IsEnabled, false)
            .With(x => x.ExpiresAt, (DateTimeOffset?)null)
            .Create();

        _storeMock.Setup(x => x.FindByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(record));
        _revocationProviderMock.Setup(x => x.IsRevokedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.ValidateAsync(rawKey);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(ApiKeyErrors.Disabled);
    }

    [Fact]
    public async Task ValidateAsync_RateLimitExceeded_ReturnsRateLimitExceededError()
    {
        // Arrange
        var rawKey = _fixture.Create<string>().PadRight(_options.MinLength, 'a');
        var record = _fixture.Build<ApiKeyRecord>()
            .With(x => x.IsEnabled, true)
            .With(x => x.ExpiresAt, (DateTimeOffset?)null)
            .Create();

        _storeMock.Setup(x => x.FindByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(record));
        _revocationProviderMock.Setup(x => x.IsRevokedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _rateLimiterMock.Setup(x => x.IsAllowedAsync(record.Id, _options.RateLimitPerMinute, _options.RateLimitWindowSeconds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.ValidateAsync(rawKey);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(ApiKeyErrors.RateLimitExceeded);
    }

    [Fact]
    public async Task ValidateAsync_ValidKey_ReturnsSuccess()
    {
        // Arrange
        var rawKey = _fixture.Create<string>().PadRight(_options.MinLength, 'a');
        var record = _fixture.Build<ApiKeyRecord>()
            .With(x => x.IsEnabled, true)
            .With(x => x.ExpiresAt, (DateTimeOffset?)null)
            .Create();

        _storeMock.Setup(x => x.FindByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(record));
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
}
