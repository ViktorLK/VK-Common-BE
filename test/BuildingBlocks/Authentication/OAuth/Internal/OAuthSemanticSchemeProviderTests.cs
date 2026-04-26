using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using VK.Blocks.Authentication.OAuth.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.UnitTests.OAuth.Internal;

public sealed class OAuthSemanticSchemeProviderTests
{
    private readonly Mock<IOptions<VKOAuthOptions>> _optionsMock;
    private readonly OAuthSemanticSchemeProvider _provider;

    public OAuthSemanticSchemeProviderTests()
    {
        _optionsMock = new Mock<IOptions<VKOAuthOptions>>();
        _provider = new OAuthSemanticSchemeProvider(_optionsMock.Object);
    }

    [Fact]
    public void GetUserSchemes_ShouldReturnEmpty_WhenDisabled()
    {
        // Arrange
        _optionsMock.Setup(x => x.Value).Returns(new VKOAuthOptions { Enabled = false });

        // Act
        var result = _provider.GetUserSchemes();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetUserSchemes_ShouldReturnProviderSchemes_WhenEnabled()
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
                    SchemeName = "GitHub-Auth",
                    ClientId = "id",
                    ClientSecret = "sec",
                    Authority = "auth",
                    CallbackPath = "/cb"
                },
                ["Google"] = new()
                {
                    Enabled = true,
                    ClientId = "id",
                    ClientSecret = "sec",
                    Authority = "auth",
                    CallbackPath = "/cb"
                },
                ["Facebook"] = new()
                {
                    Enabled = false,
                    ClientId = "id",
                    ClientSecret = "sec",
                    Authority = "auth",
                    CallbackPath = "/cb"
                }
            }
        };
        _optionsMock.Setup(x => x.Value).Returns(options);

        // Act
        var result = _provider.GetUserSchemes();

        // Assert
        result.Should().HaveCount(2).And.Contain(["GitHub-Auth", "Google"]);
    }

    [Fact]
    public void GetSchemesForPolicy_ShouldReturnEmpty_ForUnknownPolicy()
    {
        // Act
        var result = _provider.GetSchemesForPolicy("SomeOtherPolicy");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetSchemesForPolicy_ShouldReturnSchemes_ForOAuthPolicy()
    {
        // Arrange
        var options = new VKOAuthOptions
        {
            Enabled = true,
            Providers = new Dictionary<string, VKOAuthProviderOptions>
            {
                ["GitHub"] = new() { Enabled = true, ClientId = "id", ClientSecret = "sec", Authority = "auth", CallbackPath = "/cb" }
            }
        };
        _optionsMock.Setup(x => x.Value).Returns(options);

        // Act
        var result = _provider.GetSchemesForPolicy(VKAuthPolicies.OAuth);

        // Assert
        result.Should().HaveCount(1).And.Contain("GitHub");
    }
}
