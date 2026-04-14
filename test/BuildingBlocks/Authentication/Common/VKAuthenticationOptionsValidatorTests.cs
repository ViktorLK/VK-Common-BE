using FluentAssertions;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Common;
using VK.Blocks.Authentication.DependencyInjection;
using VK.Blocks.Authentication.Features.ApiKeys;
using VK.Blocks.Authentication.Features.Jwt;
using VK.Blocks.Authentication.Features.OAuth;

namespace VK.Blocks.Authentication.UnitTests.Common;

public sealed class VKAuthenticationOptionsValidatorTests
{
    private readonly VKAuthenticationOptionsValidator _sut;

    public VKAuthenticationOptionsValidatorTests()
    {
        _sut = new VKAuthenticationOptionsValidator();
    }

    [Fact]
    public void Validate_WhenDisabled_ShouldReturnSuccess()
    {
        // Arrange
        var options = new VKAuthenticationOptions { Enabled = false };

        // Act
        var result = _sut.Validate(null, options);

        // Assert
        result.Should().Be(ValidateOptionsResult.Success);
    }

    [Fact]
    public void Validate_WhenDefaultSchemeMissing_ShouldReturnFailure()
    {
        // Arrange
        var options = new VKAuthenticationOptions
        {
            Enabled = true,
            DefaultScheme = string.Empty
        };

        // Act
        var result = _sut.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Be(AuthenticationConstants.DefaultSchemeRequired);
    }

    [Fact]
    public void Validate_WhenInvalidCleanupInterval_ShouldReturnFailure()
    {
        // Arrange
        var options = new VKAuthenticationOptions
        {
            Enabled = true,
            DefaultScheme = "Bearer",
            InMemoryCleanupIntervalMinutes = 0
        };

        // Act
        var result = _sut.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Be(AuthenticationConstants.MinCleanupIntervalId);
    }

    [Fact]
    public void Validate_WhenNoStrategyEnabled_ShouldReturnFailure()
    {
        // Arrange
        var options = new VKAuthenticationOptions
        {
            Enabled = true,
            DefaultScheme = "Bearer",
            InMemoryCleanupIntervalMinutes = 1,
            Jwt = new JwtOptions { Enabled = false },
            ApiKey = new ApiKeyOptions { Enabled = false },
            OAuth = new VKOAuthOptions { Enabled = false }
        };

        // Act
        var result = _sut.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Be(AuthenticationConstants.AtLeastOneStrategyRequired);
    }

    [Theory]
    [InlineData("Jwt")]
    [InlineData("ApiKey")]
    [InlineData("OAuth")]
    public void Validate_WhenAtLeastOneStrategyEnabled_ShouldReturnSuccess(string strategy)
    {
        // Arrange
        var options = new VKAuthenticationOptions
        {
            Enabled = true,
            DefaultScheme = "Bearer",
            InMemoryCleanupIntervalMinutes = 1,
            Jwt = new JwtOptions { Enabled = strategy == "Jwt" },
            ApiKey = new ApiKeyOptions { Enabled = strategy == "ApiKey" },
            OAuth = new VKOAuthOptions { Enabled = strategy == "OAuth" }
        };

        // Act
        var result = _sut.Validate(null, options);

        // Assert
        result.Should().Be(ValidateOptionsResult.Success);
    }
}
