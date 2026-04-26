using System.Security.Claims;
using FluentAssertions;
using Microsoft.IdentityModel.JsonWebTokens;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.UnitTests.Common;

public sealed class VKClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetUserId_ShouldReturnNameIdentifier_WhenPresent()
    {
        // Arrange
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-123")]);
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetUserId();

        // Assert
        result.Should().Be("user-123");
    }

    [Fact]
    public void GetUserId_ShouldReturnVKUserId_WhenNameIdentifierIsMissing()
    {
        // Arrange
        var identity = new ClaimsIdentity([new Claim(VKClaimConstants.UserId, "vk-user-456")]);
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetUserId();

        // Assert
        result.Should().Be("vk-user-456");
    }

    [Fact]
    public void GetJti_ShouldReturnJtiClaim()
    {
        // Arrange
        var identity = new ClaimsIdentity([new Claim(VKClaimConstants.Jti, "jti-789")]);
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetJti();

        // Assert
        result.Should().Be("jti-789");
    }

    [Fact]
    public void GetTenantId_ShouldReturnVKTenantId_WhenPresent()
    {
        // Arrange
        var identity = new ClaimsIdentity([new Claim(VKClaimConstants.TenantId, "tenant-1")]);
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetTenantId();

        // Assert
        result.Should().Be("tenant-1");
    }

    [Fact]
    public void GetTenantId_ShouldReturnAzureTenantId_WhenVKTenantIdIsMissing()
    {
        // Arrange
        var identity = new ClaimsIdentity([new Claim(VKClaimConstants.AzureTenantId, "azure-tenant-2")]);
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetTenantId();

        // Assert
        result.Should().Be("azure-tenant-2");
    }

    [Fact]
    public void GetDisplayName_ShouldConcatenateGivenNameAndSurname()
    {
        // Arrange
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.GivenName, "John"),
            new Claim(ClaimTypes.Surname, "Doe")
        ]);
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetDisplayName();

        // Assert
        result.Should().Be("John Doe");
    }

    [Fact]
    public void GetDisplayName_ShouldReturnGivenName_WhenSurnameIsMissing()
    {
        // Arrange
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.GivenName, "John")]);
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetDisplayName();

        // Assert
        result.Should().Be("John");
    }

    [Fact]
    public void GetDisplayName_ShouldReturnVKName_WhenStandardNamesAreMissing()
    {
        // Arrange
        var identity = new ClaimsIdentity([new Claim(VKClaimConstants.Name, "VK Display Name")]);
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetDisplayName();

        // Assert
        result.Should().Be("VK Display Name");
    }

    [Fact]
    public void GetRoles_ShouldReturnDistinctRolesFromMultipleClaims()
    {
        // Arrange
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(VKClaimConstants.Role, "PowerUser"),
            new Claim(ClaimTypes.Role, "Admin") // Duplicate
        ]);
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetRoles();

        // Assert
        result.Should().HaveCount(2).And.Contain(["Admin", "PowerUser"]);
    }

    [Fact]
    public void ToAuthenticatedUser_ShouldReturnSuccess_WhenMandatoryClaimsPresent()
    {
        // Arrange
        var identity = new ClaimsIdentity("Test");
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "user-123"));
        identity.AddClaim(new Claim(ClaimTypes.Name, "john.doe"));
        identity.AddClaim(new Claim(ClaimTypes.Email, "john@example.com"));
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.ToAuthenticatedUser();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be("user-123");
        result.Value.Username.Should().Be("john.doe");
        result.Value.Email.Should().Be("john@example.com");
    }

    [Fact]
    public void ToAuthenticatedUser_ShouldReturnFailure_WhenIdIsMissing()
    {
        // Arrange
        var identity = new ClaimsIdentity("Test");
        identity.AddClaim(new Claim(ClaimTypes.Name, "john.doe")); // No Id
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.ToAuthenticatedUser();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(VKAuthenticationErrors.InvalidClaims);
    }

    [Fact]
    public void GetUserId_ShouldReturnSubject_WhenNameIdentifierMissing()
    {
        // Arrange
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, "subject-123") };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetUserId();

        // Assert
        result.Should().Be("subject-123");
    }

    [Fact]
    public void GetUserId_ShouldReturnNull_WhenBothMissing()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = principal.GetUserId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetUsername_ShouldReturnName_WhenPresent()
    {
        // Arrange
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "john.doe")]);
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetUsername();

        // Assert
        result.Should().Be("john.doe");
    }

    [Fact]
    public void GetEmail_ShouldReturnEmail_WhenPresent()
    {
        // Arrange
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Email, "john@example.com")]);
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetEmail();

        // Assert
        result.Should().Be("john@example.com");
    }

    [Fact]
    public void GetVKAuthenticatedUser_ShouldReturnUser_WhenVKUserIdPresent()
    {
        // Arrange
        var identity = new ClaimsIdentity("Test");
        identity.AddClaim(new Claim(VKClaimConstants.UserId, "vk-123"));
        identity.AddClaim(new Claim(VKClaimConstants.PreferredUsername, "vk.user"));
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetVKAuthenticatedUser();

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("vk-123");
        result.Username.Should().Be("vk.user");
    }

    [Fact]
    public void GetVKAuthenticatedUser_ShouldReturnNull_WhenVKUserIdMissing()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = principal.GetVKAuthenticatedUser();

        // Assert
        result.Should().BeNull();
    }
}
