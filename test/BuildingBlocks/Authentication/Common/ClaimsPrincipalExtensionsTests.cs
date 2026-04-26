using System.Security.Claims;
using FluentAssertions;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.UnitTests.Common;

public sealed class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetUserId_WithPrincipal_ShouldReturnCorrectId()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-id-123") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.GetUserId();

        // Assert
        result.Should().Be("user-id-123");
    }

    [Fact]
    public void GetUserId_WithVKUserId_ShouldReturnCorrectId()
    {
        // Arrange
        var claims = new[] { new Claim(VKClaimConstants.UserId, "vk-user-id") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.GetUserId();

        // Assert
        result.Should().Be("vk-user-id");
    }

    [Fact]
    public void GetJti_WithClaim_ShouldReturnCorrectJti()
    {
        // Arrange
        var claims = new[] { new Claim(VKClaimConstants.Jti, "jti-value") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.GetJti();

        // Assert
        result.Should().Be("jti-value");
    }

    [Fact]
    public void GetUsername_WithPreferredUsername_ShouldReturnCorrectUsername()
    {
        // Arrange
        var claims = new[] { new Claim(VKClaimConstants.PreferredUsername, "vk-user") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.GetUsername();

        // Assert
        result.Should().Be("vk-user");
    }

    [Fact]
    public void GetDisplayName_WithFirstAndLastName_ShouldFormatCorrectly()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.GivenName, "John"),
            new Claim(ClaimTypes.Surname, "Doe")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.GetDisplayName();

        // Assert
        result.Should().Be("John Doe");
    }

    private static readonly string[] expected = new[] { "Admin", "User" };

    [Fact]
    public void GetRoles_ShouldReturnUniqueRolesFromAllSources()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(VKClaimConstants.Role, "User"),
            new Claim(ClaimTypes.Role, "Admin") // Duplicate
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var roles = principal.GetRoles();

        // Assert
        roles.Should().HaveCount(2).And.Contain(expected);
    }

    [Fact]
    public void ToAuthenticatedUser_WithValidPrincipal_ShouldMapCorrectly()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-1"),
            new Claim(ClaimTypes.Name, "john.doe"),
            new Claim(ClaimTypes.Email, "john@example.com"),
            new Claim(VKClaimConstants.TenantId, "tenant-1"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim("custom-claim", "value")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.ToAuthenticatedUser();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be("user-1");
        result.Value.Username.Should().Be("john.doe");
        result.Value.Email.Should().Be("john@example.com");
        result.Value.TenantId.Should().Be("tenant-1");
        result.Value.Roles.Should().Contain("Admin");
        result.Value.Claims.Should().ContainKey("custom-claim").WhoseValue.Should().Be("value");
    }

    [Fact]
    public void ToAuthenticatedUser_WithMissingMandatoryClaims_ShouldReturnFailure()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.Email, "john@example.com") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.ToAuthenticatedUser();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(VKAuthenticationErrors.InvalidClaims);
    }

    [Fact]
    public void GetDisplayName_WithOnlyName_ShouldReturnName()
    {
        // Arrange
        var claims = new[] { new Claim(VKClaimConstants.Name, "display-name") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.GetDisplayName();

        // Assert
        result.Should().Be("display-name");
    }

    [Fact]
    public void GetTenantId_WithAzureTenantId_ShouldReturnCorrectId()
    {
        // Arrange
        var claims = new[] { new Claim(VKClaimConstants.AzureTenantId, "azure-tenant") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.GetTenantId();

        // Assert
        result.Should().Be("azure-tenant");
    }
}
