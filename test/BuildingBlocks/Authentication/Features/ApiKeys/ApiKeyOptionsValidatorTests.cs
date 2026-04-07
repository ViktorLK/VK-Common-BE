using FluentAssertions;
using VK.Blocks.Authentication.Features.ApiKeys;

namespace VK.Blocks.Authentication.UnitTests.Features.ApiKeys;

public sealed class ApiKeyOptionsValidatorTests
{
    private readonly ApiKeyOptionsValidator _validator = new();

    [Fact]
    public void Validate_WhenDisabled_ShouldReturnSuccess()
    {
        // Arrange
        var options = new ApiKeyOptions { Enabled = false };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidOptions_ShouldReturnSuccess()
    {
        // Arrange
        var options = new ApiKeyOptions 
        { 
            Enabled = true, 
            HeaderName = "X-Api-Key",
            MinLength = 10,
            EnableRateLimiting = true,
            RateLimitPerMinute = 60,
            RateLimitWindowSeconds = 60
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyHeaderName_ShouldReturnFailure()
    {
        // Arrange
        var options = new ApiKeyOptions { Enabled = true, HeaderName = "" };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("HeaderName");
    }

    [Fact]
    public void Validate_WithNegativeMinLength_ShouldReturnFailure()
    {
        // Arrange
        var options = new ApiKeyOptions { Enabled = true, HeaderName = "X-Api-Key", MinLength = -1 };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("MinLength");
    }

    [Fact]
    public void Validate_WithInvalidRateLimit_ShouldReturnFailure()
    {
        // Arrange
        var options = new ApiKeyOptions 
        { 
            Enabled = true, 
            HeaderName = "X-Api-Key",
            EnableRateLimiting = true,
            RateLimitPerMinute = 0 
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("RateLimitPerMinute");
    }
    
    [Fact]
    public void Validate_WithInvalidRateLimitWindow_ShouldReturnFailure()
    {
        // Arrange
        var options = new ApiKeyOptions 
        { 
            Enabled = true, 
            HeaderName = "X-Api-Key",
            EnableRateLimiting = true,
            RateLimitPerMinute = 60,
            RateLimitWindowSeconds = 0
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("RateLimitWindowSeconds");
    }
}
