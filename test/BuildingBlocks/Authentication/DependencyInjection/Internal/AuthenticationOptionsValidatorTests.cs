using FluentAssertions;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.DependencyInjection.Internal;

namespace VK.Blocks.Authentication.UnitTests.DependencyInjection.Internal;

public sealed class AuthenticationOptionsValidatorTests
{
    private readonly AuthenticationOptionsValidator _sut;

    public AuthenticationOptionsValidatorTests()
    {
        _sut = new AuthenticationOptionsValidator();
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
        result.FailureMessage.Should().Be(VKAuthenticationConstants.DefaultSchemeRequired);
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
        result.FailureMessage.Should().Be(VKAuthenticationConstants.MinCleanupIntervalId);
    }

    [Fact]
    public void Validate_WhenAllValid_ShouldReturnSuccess()
    {
        // Arrange
        var options = new VKAuthenticationOptions
        {
            Enabled = true,
            DefaultScheme = "Bearer",
            InMemoryCleanupIntervalMinutes = 15
        };

        // Act
        var result = _sut.Validate(null, options);

        // Assert
        result.Should().Be(ValidateOptionsResult.Success);
    }
}
