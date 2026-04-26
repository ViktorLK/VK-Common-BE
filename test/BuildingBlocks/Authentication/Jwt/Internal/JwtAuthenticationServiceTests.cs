using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Moq;
using VK.Blocks.Authentication.Jwt.Internal;

namespace VK.Blocks.Authentication.UnitTests.Jwt.Internal;

public sealed class JwtAuthenticationServiceTests
{
    private readonly Mock<IOptions<VKJwtOptions>> _optionsMock;
    private readonly Mock<ILogger<JwtAuthenticationService>> _loggerMock;
    private readonly Mock<IVKJwtRevocationProvider> _revocationProviderMock;
    private readonly JwtAuthenticationService _sut;
    private readonly VKJwtOptions _options;
    private readonly string _secretKey = new('a', 32);

    public JwtAuthenticationServiceTests()
    {
        _optionsMock = new Mock<IOptions<VKJwtOptions>>();
        _loggerMock = new Mock<ILogger<JwtAuthenticationService>>();
        _revocationProviderMock = new Mock<IVKJwtRevocationProvider>();

        _options = new VKJwtOptions
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            SecretKey = _secretKey,
            AuthMode = VKJwtAuthMode.Symmetric,
            ExpiryMinutes = 60
        };

        _optionsMock.Setup(x => x.Value).Returns(_options);

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
        result.Errors.Should().Contain(VKJwtErrors.EmptyToken);
    }

    [Fact]
    public async Task AuthenticateAsync_MissingSecretKey_ReturnsConfigurationError()
    {
        // Arrange
        var options = _options with { SecretKey = string.Empty };
        var optionsMock = new Mock<IOptions<VKJwtOptions>>();
        optionsMock.Setup(x => x.Value).Returns(options);
        
        var sut = new JwtAuthenticationService(
            optionsMock.Object,
            _loggerMock.Object,
            _revocationProviderMock.Object);

        // Act
        var result = await sut.AuthenticateAsync("some-token");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(VKJwtErrors.ConfigurationError);
    }

    [Fact]
    public async Task AuthenticateAsync_InvalidToken_ReturnsInvalidError()
    {
        // Act
        var result = await _sut.AuthenticateAsync("invalid.token.format");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(VKJwtErrors.Invalid);
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
        result.Errors.Should().Contain(VKJwtErrors.Expired);
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
        result.Errors.Should().Contain(VKJwtErrors.Revoked);
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
        result.Errors.Should().Contain(VKJwtErrors.Revoked);
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
        result2.Errors.Should().Contain(VKJwtErrors.Revoked);
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
        result.Errors.Should().Contain(VKJwtErrors.Invalid);
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
        result.Errors.Should().Contain(VKAuthenticationErrors.InvalidClaims);
    }

    [Fact]
    public async Task AuthenticateAsync_UnexpectedException_ReturnsInvalidError()
    {
        // Arrange
        var userId = "user-123";
        var token = GenerateToken(DateTime.UtcNow, DateTime.UtcNow.AddMinutes(60), userId: userId);
        
        // Mock a dependency to throw during method execution
        _revocationProviderMock.Setup(x => x.IsUserRevokedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected database error"));

        // Act
        var result = await _sut.AuthenticateAsync(token);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(VKJwtErrors.Invalid);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenBothJtiAndUserRevoked_ReturnsRevokedError()
    {
        // Arrange
        var userId = "user-123";
        var jti = "token-123";
        var token = GenerateToken(DateTime.UtcNow, DateTime.UtcNow.AddMinutes(60), userId: userId, jti: jti);

        // Both report as revoked
        _revocationProviderMock.Setup(x => x.IsUserRevokedAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _revocationProviderMock.Setup(x => x.IsRevokedAsync(jti, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.AuthenticateAsync(token);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(VKJwtErrors.Revoked);
        // Should stop at first revocation check (typically JTI)
        _revocationProviderMock.Verify(x => x.IsRevokedAsync(jti, It.IsAny<CancellationToken>()), Times.AtMostOnce);
    }

    [Fact]
    public async Task ValidateRevocationAsync_WithNullPrincipal_ReturnsSuccess()
    {
        // Act
        var result = await _sut.ValidateRevocationAsync(null!);

        // Assert
        result.IsSuccess.Should().BeTrue("Validating revocation for null principal should bypass check and return success");
    }

    [Fact]
    public async Task ValidateRevocationAsync_WithMissingUserId_ReturnsSuccess()
    {
        // Arrange
        var identity = new ClaimsIdentity(); // No claims
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _sut.ValidateRevocationAsync(principal);

        // Assert
        result.IsSuccess.Should().BeTrue("No user ID means no user-based revocation check needed");
        _revocationProviderMock.Verify(x => x.IsUserRevokedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    #region Helpers

    private string GenerateToken(DateTime nbf, DateTime exp, string? userId = "test-user", string? username = "test-name", string? jti = "test-jti", string? secretKey = null)
    {
        var handler = new JsonWebTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey ?? _secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>();
        if (userId != null)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        if (username != null)
            claims.Add(new Claim(ClaimTypes.Name, username));
        if (jti != null)
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, jti));

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
