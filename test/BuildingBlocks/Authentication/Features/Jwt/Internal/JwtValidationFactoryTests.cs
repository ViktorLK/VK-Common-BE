using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using VK.Blocks.Authentication.Abstractions;
using VK.Blocks.Authentication.Features.Jwt;
using VK.Blocks.Authentication.Features.Jwt.Internal;
using VK.Blocks.Authentication.Features.Jwt.Metadata;

namespace VK.Blocks.Authentication.UnitTests.Features.Jwt.Internal;

public sealed class JwtValidationFactoryTests
{
    [Fact]
    public void Create_WithSymmetricMode_ShouldReturnConfiguredParameters()
    {
        // Arrange
        var options = new JwtOptions
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            SecretKey = "super-secret-key-that-is-long-enough",
            AuthMode = JwtAuthMode.Symmetric,
            ClockSkewSeconds = 30
        };

        // Act
        var parameters = JwtValidationFactory.Create(options);

        // Assert
        parameters.Should().NotBeNull();
        parameters.ValidateIssuer.Should().BeTrue();
        parameters.ValidateAudience.Should().BeTrue();
        parameters.ValidateLifetime.Should().BeTrue();
        parameters.ValidateIssuerSigningKey.Should().BeTrue();
        parameters.ValidIssuer.Should().Be(options.Issuer);
        parameters.ValidAudience.Should().Be(options.Audience);
        parameters.ClockSkew.Should().Be(TimeSpan.FromSeconds(options.ClockSkewSeconds));
        parameters.NameClaimType.Should().Be(VKClaimTypes.Name);
        parameters.RoleClaimType.Should().Be(VKClaimTypes.Role);
        parameters.IssuerSigningKey.Should().BeOfType<SymmetricSecurityKey>();
    }

    [Fact]
    public void Create_WithAsymmetricMode_ShouldNotSetIssuerSigningKey()
    {
        // Arrange
        var options = new JwtOptions
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            AuthMode = JwtAuthMode.OidcDiscovery,
            ClockSkewSeconds = 30
        };

        // Act
        var parameters = JwtValidationFactory.Create(options);

        // Assert
        parameters.Should().NotBeNull();
        parameters.IssuerSigningKey.Should().BeNull();
    }
}
