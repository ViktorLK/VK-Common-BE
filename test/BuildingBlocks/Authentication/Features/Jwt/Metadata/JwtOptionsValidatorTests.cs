using FluentAssertions;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Features.Jwt;
using VK.Blocks.Authentication.Features.Jwt.Internal;
using VK.Blocks.Authentication.Features.Jwt.Metadata;
using Xunit;

namespace VK.Blocks.Authentication.UnitTests.Features.Jwt.Metadata;

public sealed class JwtOptionsValidatorTests
{
    private readonly JwtOptionsValidator _validator;

    public JwtOptionsValidatorTests()
    {
        _validator = new JwtOptionsValidator();
    }

    [Fact]
    public void Validate_ValidSymmetricOptions_ReturnsSuccess()
    {
        // Arrange
        var options = new JwtOptions
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AuthMode = JwtAuthMode.Symmetric,
            SecretKey = new string('a', 32),
            ExpiryMinutes = 60,
            RefreshTokenLifetimeDays = 7
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_ValidOidcOptions_ReturnsSuccess()
    {
        // Arrange
        var options = new JwtOptions
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AuthMode = JwtAuthMode.OidcDiscovery,
            Authority = "https://example.com"
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_MissingIssuer_ReturnsFail(string? issuer)
    {
        // Arrange
        var options = new JwtOptions { Enabled = true, Issuer = issuer!, Audience = "TestAudience" };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Be(JwtConstants.IssuerRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_MissingAudience_ReturnsFail(string? audience)
    {
        // Arrange
        var options = new JwtOptions { Enabled = true, Issuer = "TestIssuer", Audience = audience! };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Be(JwtConstants.AudienceRequired);
    }

    [Fact]
    public void Validate_NegativeClockSkew_ReturnsFail()
    {
        // Arrange
        var options = new JwtOptions { Enabled = true, Issuer = "Issuer", Audience = "Audience", ClockSkewSeconds = -1 };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Be(JwtConstants.ClockSkewInvalid);
    }

    [Fact]
    public void Validate_SymmetricMissingSecretKey_ReturnsFail()
    {
        // Arrange
        var options = new JwtOptions 
        { 
            Enabled = true,
            Issuer = "Issuer", 
            Audience = "Audience", 
            AuthMode = JwtAuthMode.Symmetric,
            SecretKey = "" 
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public void Validate_SymmetricShortSecretKey_ReturnsFail()
    {
        // Arrange
        var options = new JwtOptions 
        { 
            Enabled = true,
            Issuer = "Issuer", 
            Audience = "Audience", 
            AuthMode = JwtAuthMode.Symmetric,
            SecretKey = "short" 
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1000000)]
    public void Validate_SymmetricInvalidExpiry_ReturnsFail(int expiry)
    {
        // Arrange
        var options = new JwtOptions 
        { 
            Enabled = true,
            Issuer = "Issuer", 
            Audience = "Audience", 
            AuthMode = JwtAuthMode.Symmetric,
            SecretKey = new string('a', 32),
            ExpiryMinutes = expiry
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1000)]
    public void Validate_SymmetricInvalidRefreshTokenLifetime_ReturnsFail(int days)
    {
        // Arrange
        var options = new JwtOptions 
        { 
            Enabled = true,
            Issuer = "Issuer", 
            Audience = "Audience", 
            AuthMode = JwtAuthMode.Symmetric,
            SecretKey = new string('a', 32),
            ExpiryMinutes = 60,
            RefreshTokenLifetimeDays = days
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public void Validate_OidcMissingAuthority_ReturnsFail()
    {
        // Arrange
        var options = new JwtOptions 
        { 
            Enabled = true,
            Issuer = "Issuer", 
            Audience = "Audience", 
            AuthMode = JwtAuthMode.OidcDiscovery,
            Authority = "" 
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Be(JwtConstants.AuthorityRequired);
    }

    [Fact]
    public void Validate_InvalidAuthMode_ReturnsFail()
    {
        // Arrange
        var options = new JwtOptions 
        { 
            Enabled = true,
            Issuer = "Issuer", 
            Audience = "Audience", 
            AuthMode = (JwtAuthMode)999
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Be(JwtConstants.InvalidAuthMode);
    }
}
