using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Moq;
using VK.Blocks.Authentication.Common;
using VK.Blocks.Authentication.Common.Extensions;
using VK.Blocks.Authentication.Features.Jwt;
using VK.Blocks.Authentication.Features.Jwt.RefreshTokens;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authentication.UnitTests.Features.Jwt;

public sealed class JwtAuthenticationServiceTests
{
    private readonly Mock<IOptionsMonitor<JwtOptions>> _optionsMock;
    private readonly Mock<ILogger<JwtAuthenticationService>> _loggerMock;
    private readonly Mock<IJwtTokenRevocationProvider> _revocationProviderMock;
    private readonly JwtAuthenticationService _sut;
    private readonly JwtOptions _options;
    private readonly string _secretKey = new('a', 32);

    public JwtAuthenticationServiceTests()
    {
        _optionsMock = new Mock<IOptionsMonitor<JwtOptions>>();
        _loggerMock = new Mock<ILogger<JwtAuthenticationService>>();
        _revocationProviderMock = new Mock<IJwtTokenRevocationProvider>();

        _options = new JwtOptions
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            SecretKey = _secretKey,
            AuthMode = JwtAuthMode.Symmetric,
            ExpiryMinutes = 60
        };

        _optionsMock.Setup(x => x.CurrentValue).Returns(_options);

        _sut = new JwtAuthenticationService(
            _optionsMock.Object,
            _loggerMock.Object,
            _revocationProviderMock.Object);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AuthenticateAsync_EmptyToken_ReturnsEmptyTokenError(string? token)
    {
        // Act
        var result = await _sut.AuthenticateAsync(token!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(JwtErrors.EmptyToken);
    }

    [Fact]
    public async Task AuthenticateAsync_MissingSecretKey_ReturnsConfigurationError()
    {
        // Arrange
        _options.SecretKey = string.Empty;

        // Act
        var result = await _sut.AuthenticateAsync("some-token");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(JwtErrors.ConfigurationError);
    }

    [Fact]
    public async Task AuthenticateAsync_InvalidToken_ReturnsInvalidError()
    {
        // Act
        var result = await _sut.AuthenticateAsync("invalid.token.format");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(JwtErrors.Invalid);
    }

    [Fact]
    public async Task AuthenticateAsync_ExpiredToken_ReturnsExpiredError()
    {
        // Arrange
        var token = GenerateToken(DateTime.UtcNow.AddMinutes(-30), DateTime.UtcNow.AddMinutes(-10));

        // Act
        var result = await _sut.AuthenticateAsync(token);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(JwtErrors.Expired);
    }

    [Fact]
    public async Task AuthenticateAsync_UserRevoked_ReturnsRevokedError()
    {
        // Arrange
        var userId = "user-123";
        var token = GenerateToken(DateTime.UtcNow, DateTime.UtcNow.AddMinutes(60), userId: userId);
        _revocationProviderMock.Setup(x => x.IsUserRevokedAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.AuthenticateAsync(token);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(JwtErrors.Revoked);
    }

    [Fact]
    public async Task AuthenticateAsync_TokenRevokedByJti_ReturnsRevokedError()
    {
        // Arrange
        var jti = "token-123";
        var token = GenerateToken(DateTime.UtcNow, DateTime.UtcNow.AddMinutes(60), jti: jti);
        _revocationProviderMock.Setup(x => x.IsUserRevokedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _revocationProviderMock.Setup(x => x.IsRevokedAsync(jti, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.AuthenticateAsync(token);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(JwtErrors.Revoked);
        _revocationProviderMock.Verify(x => x.IsRevokedAsync(jti, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_ValidToken_ReturnsSuccessWithUser()
    {
        // Arrange
        var userId = "user-123";
        var username = "john.doe";
        var token = GenerateToken(DateTime.UtcNow, DateTime.UtcNow.AddMinutes(60), userId: userId, username: username);
        _revocationProviderMock.Setup(x => x.IsUserRevokedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _revocationProviderMock.Setup(x => x.IsRevokedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.AuthenticateAsync(token);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(userId);
        result.Value!.Username.Should().Be(username);
    }

    [Fact]
    public async Task ValidateRevocationAsync_CachedResult_ReturnsImmediately()
    {
        // Arrange
        var userId = "user-123";
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        _revocationProviderMock.Setup(x => x.IsUserRevokedAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        // 1st call - hits provider
        var result1 = await _sut.ValidateRevocationAsync(principal);
        // 2nd call - hits cache
        var result2 = await _sut.ValidateRevocationAsync(principal);

        // Assert
        result1.IsFailure.Should().BeTrue();
        result2.IsFailure.Should().BeTrue();
        result2.Errors.Should().Contain(JwtErrors.Revoked);
        _revocationProviderMock.Verify(x => x.IsUserRevokedAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_InvalidSignature_ReturnsInvalidError()
    {
        // Arrange
        var otherSecretKey = new string('b', 32);
        var token = GenerateToken(DateTime.UtcNow, DateTime.UtcNow.AddMinutes(60), secretKey: otherSecretKey);

        // Act
        var result = await _sut.AuthenticateAsync(token);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(JwtErrors.Invalid);
    }

    [Fact]
    public async Task AuthenticateAsync_MissingSubClaim_ReturnsMappingError()
    {
        // Arrange
        // Generating token with null userId will omit the NameIdentifier claim
        var token = GenerateToken(DateTime.UtcNow, DateTime.UtcNow.AddMinutes(60), userId: null, username: null);

        // Act
        var result = await _sut.AuthenticateAsync(token);

        // Assert
        result.IsFailure.Should().BeTrue();
        // Since ToAuthenticatedUser fails if Id or Username is missing
        result.Errors.Should().Contain(AuthenticationErrors.InvalidClaims);
    }

    [Fact]
    public async Task AuthenticateAsync_UnexpectedException_ReturnsInvalidError()
    {
        // Arrange
        _optionsMock.Setup(x => x.CurrentValue).Throws(new Exception("Unexpected error"));

        // Act
        var result = await _sut.AuthenticateAsync("some-token");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(JwtErrors.Invalid);
    }
    #region Helpers

    private string GenerateToken(DateTime nbf, DateTime exp, string? userId = "test-user", string? username = "test-name", string? jti = "test-jti", string? secretKey = null)
    {
        var handler = new JsonWebTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey ?? _secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>();
        if (userId != null) claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        if (username != null) claims.Add(new Claim(ClaimTypes.Name, username));
        if (jti != null) claims.Add(new Claim(JwtRegisteredClaimNames.Jti, jti));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Subject = new ClaimsIdentity(claims),
            NotBefore = nbf,
            Expires = exp,
            SigningCredentials = credentials
        };

        return handler.CreateToken(descriptor);
    }

    #endregion
}
