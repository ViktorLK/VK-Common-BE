using System.Security.Claims;
using FluentAssertions;
using VK.Blocks.Authentication.Abstractions;
using VK.Blocks.Authentication.Features.OAuth;
using VK.Blocks.Authentication.Features.OAuth.Internal;
using VK.Blocks.Authentication.Features.OAuth.Mappers;

namespace VK.Blocks.Authentication.UnitTests.Features.OAuth.Mappers;

public sealed class GitHubClaimsMapperTests
{
    private readonly GitHubClaimsMapper _sut;

    public GitHubClaimsMapperTests()
    {
        _sut = new GitHubClaimsMapper();
    }

    [Fact]
    public void MapToClaims_BaseClaims_ShouldBeMappedCorrectly()
    {
        // Arrange
        var userInfo = new ExternalIdentity
        {
            Provider = OAuthConstants.GitHub,
            ProviderId = "12345",
            Name = "John Doe",
            Email = "john@example.com"
        };

        // Act
        var result = _sut.MapToClaims(userInfo).ToList();

        // Assert
        result.Should().Contain(c => c.Type == VKClaimTypes.UserId && c.Value == "12345");
        result.Should().Contain(c => c.Type == VKClaimTypes.AuthType && c.Value == OAuthConstants.GitHub);
        result.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "john@example.com");
        result.Should().Contain(c => c.Type == VKClaimTypes.Name && c.Value == "John Doe");
    }

    [Fact]
    public void MapToClaims_GitHubSpecificClaims_ShouldBeMappedCorrectly()
    {
        // Arrange
        var userInfo = new ExternalIdentity
        {
            Provider = OAuthConstants.GitHub,
            ProviderId = "12345",
            Claims = new Dictionary<string, string>
            {
                { "avatar_url", "https://github.com/avatar.png" },
                { "html_url", "https://github.com/johndoe" }
            }
        };

        // Act
        var result = _sut.MapToClaims(userInfo).ToList();

        // Assert
        result.Should().Contain(c => c.Type == VKClaimTypes.AvatarUrl && c.Value == "https://github.com/avatar.png");
        result.Should().Contain(c => c.Type == VKClaimTypes.ProfileUrl && c.Value == "https://github.com/johndoe");
    }

    [Fact]
    public void MapToClaims_FallbackUrl_ShouldBeMappedCorrectly()
    {
        // Arrange
        var userInfo = new ExternalIdentity
        {
            Provider = OAuthConstants.GitHub,
            ProviderId = "12345",
            Claims = new Dictionary<string, string>
            {
                { "url", "https://api.github.com/users/johndoe" }
            }
        };

        // Act
        var result = _sut.MapToClaims(userInfo).ToList();

        // Assert
        result.Should().Contain(c => c.Type == VKClaimTypes.ProfileUrl && c.Value == "https://api.github.com/users/johndoe");
    }
}
