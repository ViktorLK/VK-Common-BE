using VK.Blocks.Core;

namespace VK.Blocks.Core.UnitTests.Context;

public sealed class VKExecutionContextTests
{
    private sealed record TestTenantCoordinate(VKTenantId TenantId) : IVKTenantCoordinate;
    private sealed record TestUserCoordinate(VKUserId UserId) : IVKUserCoordinate;

    [Fact]
    public void ForTenant_ValidCoordinate_CreatesContextWithTenantOnly()
    {
        // Arrange
        var tenant = new TestTenantCoordinate(VKTenantId.Default);

        // Act
        var context = VKExecutionContext.ForTenant(tenant);

        // Assert
        context.Tenant.Should().Be(tenant);
        context.User.Should().BeNull();
        context.HasTenant.Should().BeTrue();
        context.HasUser.Should().BeFalse();
        context.TenantId.Should().Be(VKTenantId.Default);
        context.UserId.Should().BeNull();
    }

    [Fact]
    public void ForUser_ValidCoordinate_CreatesContextWithUserOnly()
    {
        // Arrange
        var user = new TestUserCoordinate(VKUserId.System);

        // Act
        var context = VKExecutionContext.ForUser(user);

        // Assert
        context.User.Should().Be(user);
        context.Tenant.Should().BeNull();
        context.HasUser.Should().BeTrue();
        context.HasTenant.Should().BeFalse();
        context.UserId.Should().Be(VKUserId.System);
        context.TenantId.Should().BeNull();
    }

    [Fact]
    public void ForTenantUser_BothValid_CreatesContextWithBothCoordinates()
    {
        // Arrange
        var tenant = new TestTenantCoordinate(VKTenantId.Default);
        var user = new TestUserCoordinate(VKUserId.Anonymous);

        // Act
        var context = VKExecutionContext.ForTenantUser(tenant, user);

        // Assert
        context.Tenant.Should().Be(tenant);
        context.User.Should().Be(user);
        context.HasTenant.Should().BeTrue();
        context.HasUser.Should().BeTrue();
        context.TenantId.Should().Be(VKTenantId.Default);
        context.UserId.Should().Be(VKUserId.Anonymous);
    }

    [Fact]
    public void WithTenant_DerivesNewContextWithUpdatedTenant()
    {
        // Arrange
        var originalTenant = new TestTenantCoordinate(VKTenantId.Default);
        var newTenant = new TestTenantCoordinate(new VKTenantId(Guid.NewGuid()));
        var user = new TestUserCoordinate(VKUserId.System);
        var original = VKExecutionContext.ForTenantUser(originalTenant, user);

        // Act
        var updated = original.WithTenant(newTenant);

        // Assert
        updated.Should().NotBeSameAs(original);
        updated.Tenant.Should().Be(newTenant);
        updated.User.Should().Be(user);
        original.Tenant.Should().Be(originalTenant);
    }

    [Fact]
    public void WithUser_DerivesNewContextWithUpdatedUser()
    {
        // Arrange
        var tenant = new TestTenantCoordinate(VKTenantId.Default);
        var originalUser = new TestUserCoordinate(VKUserId.Anonymous);
        var newUser = new TestUserCoordinate(VKUserId.System);
        var original = VKExecutionContext.ForTenantUser(tenant, originalUser);

        // Act
        var updated = original.WithUser(newUser);

        // Assert
        updated.Should().NotBeSameAs(original);
        updated.User.Should().Be(newUser);
        updated.Tenant.Should().Be(tenant);
        original.User.Should().Be(originalUser);
    }

    [Fact]
    public void TryGetTenantId_WhenPresent_ReturnsTrueAndSetsOutParam()
    {
        // Arrange
        var tenant = new TestTenantCoordinate(VKTenantId.Default);
        var context = VKExecutionContext.ForTenant(tenant);

        // Act
        var success = context.TryGetTenantId(out var extractedTenantId);

        // Assert
        success.Should().BeTrue();
        extractedTenantId.Should().Be(VKTenantId.Default);
    }

    [Fact]
    public void TryGetTenantId_WhenNotPresent_ReturnsFalseAndDefault()
    {
        // Arrange
        var user = new TestUserCoordinate(VKUserId.System);
        var context = VKExecutionContext.ForUser(user);

        // Act
        var success = context.TryGetTenantId(out var extractedTenantId);

        // Assert
        success.Should().BeFalse();
        extractedTenantId.Should().Be(default(VKTenantId));
    }

    [Fact]
    public void TryGetUserId_WhenPresent_ReturnsTrueAndSetsOutParam()
    {
        // Arrange
        var user = new TestUserCoordinate(VKUserId.System);
        var context = VKExecutionContext.ForUser(user);

        // Act
        var success = context.TryGetUserId(out var extractedUserId);

        // Assert
        success.Should().BeTrue();
        extractedUserId.Should().Be(VKUserId.System);
    }

    [Fact]
    public void TryGetUserId_WhenNotPresent_ReturnsFalseAndDefault()
    {
        // Arrange
        var tenant = new TestTenantCoordinate(VKTenantId.Default);
        var context = VKExecutionContext.ForTenant(tenant);

        // Act
        var success = context.TryGetUserId(out var extractedUserId);

        // Assert
        success.Should().BeFalse();
        extractedUserId.Should().Be(default(VKUserId));
    }
}
