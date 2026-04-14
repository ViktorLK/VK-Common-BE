using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Features.OAuth;
using VK.Blocks.Authentication.Features.OAuth.Internal;
using FluentAssertions;

namespace VK.Blocks.Authentication.UnitTests.Features.OAuth;

public sealed class VKOAuthOptionsValidatorTests
{
    private readonly VKOAuthOptionsValidator _validator = new();

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
            Providers = new Dictionary<string, OAuthProviderOptions>()
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
            Providers = new Dictionary<string, OAuthProviderOptions>
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
            Providers = new Dictionary<string, OAuthProviderOptions>
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
            Providers = new Dictionary<string, OAuthProviderOptions>
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
            Providers = new Dictionary<string, OAuthProviderOptions>
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
            Providers = new Dictionary<string, OAuthProviderOptions>
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
