using System;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using VK.Blocks.Authentication.Jwt.Internal;
using VK.Blocks.Core;
namespace VK.Blocks.Authentication.UnitTests.Jwt.Internal;

public sealed class JwtValidationFactoryTests
{
    [Fact]
    public void Create_WithSymmetricMode_ShouldReturnConfiguredParameters()
    {
        // Arrange
        var options = new VKJwtOptions
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            SecretKey = "super-secret-key-that-is-long-enough",
            AuthMode = VKJwtAuthMode.Symmetric,
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
        parameters.NameClaimType.Should().Be(VKClaimConstants.Name);
        parameters.RoleClaimType.Should().Be(VKClaimConstants.Role);
        parameters.IssuerSigningKey.Should().BeOfType<SymmetricSecurityKey>();
    }

    [Fact]
    public void Create_WithAsymmetricMode_ShouldNotSetIssuerSigningKey()
    {
        // Arrange
        var options = new VKJwtOptions
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            AuthMode = VKJwtAuthMode.OidcDiscovery,
            ClockSkewSeconds = 30
        };

        // Act
        var parameters = JwtValidationFactory.Create(options);

        // Assert
        parameters.Should().NotBeNull();
        parameters.IssuerSigningKey.Should().BeNull();
    }
}
