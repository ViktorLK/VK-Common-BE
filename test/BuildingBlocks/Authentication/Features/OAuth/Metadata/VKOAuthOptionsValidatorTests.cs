using FluentAssertions;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Features.OAuth;
using VK.Blocks.Authentication.Features.OAuth.Internal;

namespace VK.Blocks.Authentication.UnitTests.Features.OAuth.Metadata;

public sealed class VKOAuthOptionsValidatorTests
{
    private readonly VKOAuthOptionsValidator _sut;

    public VKOAuthOptionsValidatorTests()
    {
        _sut = new VKOAuthOptionsValidator();
    }

    [Fact]
    public void Validate_WhenDisabled_ShouldReturnSuccess()
    {
        // Arrange
        var options = new VKOAuthOptions { Enabled = false };

        // Act
        var result = _sut.Validate(null, options);

        // Assert
        result.Should().Be(ValidateOptionsResult.Success);
    }

    [Fact]
    public void Validate_WhenEnabledAndNoProviders_ShouldReturnFailure()
    {
        // Arrange
        var options = new VKOAuthOptions 
        { 
            Enabled = true,
            Providers = []
        };

        // Act
        var result = _sut.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Be(VKOAuthErrors.MissingProviders);
    }

    [Theory]
    [InlineData("ClientId", "")]
    [InlineData("ClientId", "  ")]
    [InlineData("ClientSecret", "")]
    [InlineData("Authority", "")]
    [InlineData("CallbackPath", "")]
    public void Validate_WhenEnabledProviderHasMissingFields_ShouldReturnFailure(string field, string value)
    {
        // Arrange
        var provider = new OAuthProviderOptions
        {
            Enabled = true,
            ClientId = field == "ClientId" ? value : "valid-id",
            ClientSecret = field == "ClientSecret" ? value : "valid-secret",
            Authority = field == "Authority" ? value : "https://valid-authority",
            CallbackPath = field == "CallbackPath" ? value : "/callback"
        };

        var options = new VKOAuthOptions
        {
            Enabled = true,
            Providers = new Dictionary<string, OAuthProviderOptions> { { "TestProvider", provider } }
        };

        // Act
        var result = _sut.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("TestProvider");
    }

    [Fact]
    public void Validate_WhenEnabledProvidersAreValid_ShouldReturnSuccess()
    {
        // Arrange
        var provider = new OAuthProviderOptions
        {
            Enabled = true,
            ClientId = "valid-id",
            ClientSecret = "valid-secret",
            Authority = "https://valid-authority",
            CallbackPath = "/callback",
            SchemeName = "CustomGitHub", // Test SchemeName getter
            ResponseType = "code",       // Test ResponseType getter
            GetClaimsFromUserInfoEndpoint = true // Test GetClaimsFromUserInfoEndpoint getter
        };

        var options = new VKOAuthOptions
        {
            Enabled = true,
            Providers = new Dictionary<string, OAuthProviderOptions> { { "GitHub", provider } }
        };

        // Act
        var result = _sut.Validate(null, options);

        // Assert
        result.Should().Be(ValidateOptionsResult.Success);
        provider.SchemeName.Should().Be("CustomGitHub");
        provider.ResponseType.Should().Be("code");
        provider.GetClaimsFromUserInfoEndpoint.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenDisabledProviderIsInvalid_ShouldReturnSuccess()
    {
        // Arrange
        var provider = new OAuthProviderOptions
        {
            Enabled = false,
            ClientId = "", // Invalid but disabled
            ClientSecret = "valid-secret",
            Authority = "https://valid-authority",
            CallbackPath = "/callback"
        };

        var options = new VKOAuthOptions
        {
            Enabled = true,
            Providers = new Dictionary<string, OAuthProviderOptions> { { "GitHub", provider } }
        };

        // Act
        var result = _sut.Validate(null, options);

        // Assert
        result.Should().Be(ValidateOptionsResult.Success);
    }
}
