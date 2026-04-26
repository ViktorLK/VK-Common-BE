using System.Collections.Generic;
using FluentAssertions;
using VK.Blocks.Authentication.OAuth.Internal;

namespace VK.Blocks.Authentication.UnitTests.OAuth.Internal;

public sealed class OAuthOptionsValidatorTests
{
    private readonly OAuthOptionsValidator _validator = new();

    [Fact]
    public void Validate_WhenOAuthDisabled_ReturnsSuccess()
    {
        // Arrange
        var options = new VKOAuthOptions { Enabled = false };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenOAuthEnabledButNoProviders_ReturnsFailure()
    {
        // Arrange
        var options = new VKOAuthOptions
        {
            Enabled = true,
            Providers = new Dictionary<string, VKOAuthProviderOptions>()
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Be(VKOAuthErrors.MissingProviders);
    }

    [Fact]
    public void Validate_WhenProviderMissingClientId_ReturnsFailure()
    {
        // Arrange
        var options = new VKOAuthOptions
        {
            Enabled = true,
            Providers = new Dictionary<string, VKOAuthProviderOptions>
            {
                ["GitHub"] = new()
                {
                    Enabled = true,
                    ClientId = "", // Missing (empty)
                    ClientSecret = "secret",
                    Authority = "auth",
                    CallbackPath = "/cb"
                }
            }
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain(string.Format(VKOAuthErrors.MissingClientIdTemplate, "GitHub"));
    }

    [Fact]
    public void Validate_WhenProviderMissingSecret_ReturnsFailure()
    {
        // Arrange
        var options = new VKOAuthOptions
        {
            Enabled = true,
            Providers = new Dictionary<string, VKOAuthProviderOptions>
            {
                ["GitHub"] = new()
                {
                    Enabled = true,
                    ClientId = "id",
                    ClientSecret = "", // Missing (empty)
                    Authority = "auth",
                    CallbackPath = "/cb"
                }
            }
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain(string.Format(VKOAuthErrors.MissingClientSecretTemplate, "GitHub"));
    }

    [Fact]
    public void Validate_WhenProviderMissingAuthority_ReturnsFailure()
    {
        // Arrange
        var options = new VKOAuthOptions
        {
            Enabled = true,
            Providers = new Dictionary<string, VKOAuthProviderOptions>
            {
                ["GitHub"] = new()
                {
                    Enabled = true,
                    ClientId = "id",
                    ClientSecret = "secret",
                    Authority = "", // Missing (empty)
                    CallbackPath = "/cb"
                }
            }
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain(string.Format(VKOAuthErrors.MissingAuthorityTemplate, "GitHub"));
    }

    [Fact]
    public void Validate_WhenProviderMissingCallback_ReturnsFailure()
    {
        // Arrange
        var options = new VKOAuthOptions
        {
            Enabled = true,
            Providers = new Dictionary<string, VKOAuthProviderOptions>
            {
                ["GitHub"] = new()
                {
                    Enabled = true,
                    ClientId = "id",
                    ClientSecret = "secret",
                    Authority = "auth",
                    CallbackPath = "" // Missing (empty)
                }
            }
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain(string.Format(VKOAuthErrors.MissingCallbackPathTemplate, "GitHub"));
    }

    [Fact]
    public void Validate_WhenValidProvider_ReturnsSuccess()
    {
        // Arrange
        var options = new VKOAuthOptions
        {
            Enabled = true,
            Providers = new Dictionary<string, VKOAuthProviderOptions>
            {
                ["GitHub"] = new()
                {
                    Enabled = true,
                    ClientId = "id",
                    ClientSecret = "secret",
                    Authority = "auth",
                    CallbackPath = "/cb"
                }
            }
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }
}
