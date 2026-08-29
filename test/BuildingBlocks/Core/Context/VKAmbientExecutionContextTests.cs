using VK.Blocks.Core;

namespace VK.Blocks.Core.UnitTests.Context;

public sealed class VKAmbientExecutionContextTests
{
    private sealed record TestTenantCoordinate(VKTenantId TenantId) : IVKTenantCoordinate;
    private sealed record TestUserCoordinate(VKUserId UserId) : IVKUserCoordinate;

    [Fact]
    public void BeginScope_StronglyTypedTenantId_PushesAndRestoresScope()
    {
        // Arrange
        var tenantId = VKTenantId.Default;

        // Act & Assert (Before)
        VKAmbientExecutionContext.HasContext.Should().BeFalse();
        VKAmbientExecutionContext.Current.Should().BeNull();

        using (VKAmbientExecutionContext.BeginScope(tenantId))
        {
            VKAmbientExecutionContext.HasContext.Should().BeTrue();
            VKAmbientExecutionContext.Current!.TenantId.Should().Be(tenantId);
            VKAmbientExecutionContext.Current.UserId.Should().BeNull();
        }

        // Assert (After)
        VKAmbientExecutionContext.HasContext.Should().BeFalse();
        VKAmbientExecutionContext.Current.Should().BeNull();
    }

    [Fact]
    public void BeginScope_StronglyTypedUserId_PushesAndRestoresScope()
    {
        // Arrange
        var userId = VKUserId.System;

        // Act & Assert
        using (VKAmbientExecutionContext.BeginScope(userId))
        {
            VKAmbientExecutionContext.HasContext.Should().BeTrue();
            VKAmbientExecutionContext.Current!.UserId.Should().Be(userId);
            VKAmbientExecutionContext.Current.TenantId.Should().BeNull();
        }

        VKAmbientExecutionContext.HasContext.Should().BeFalse();
    }

    [Fact]
    public void BeginScope_TenantAndUserCoordinates_PushesBothCoordinates()
    {
        // Arrange
        var tenantId = VKTenantId.Default;
        var userId = VKUserId.Anonymous;

        // Act & Assert
        using (VKAmbientExecutionContext.BeginScope(tenantId, userId))
        {
            VKAmbientExecutionContext.HasContext.Should().BeTrue();
            VKAmbientExecutionContext.Current!.TenantId.Should().Be(tenantId);
            VKAmbientExecutionContext.Current.UserId.Should().Be(userId);
        }

        VKAmbientExecutionContext.HasContext.Should().BeFalse();
    }

    [Fact]
    public void BeginScope_NestedScopes_RestoresOuterScopeCorrectly()
    {
        // Arrange
        var outerTenant = VKTenantId.Default;
        var innerTenant = new VKTenantId(Guid.NewGuid());

        // Act & Assert
        using (VKAmbientExecutionContext.BeginScope(outerTenant))
        {
            VKAmbientExecutionContext.Current!.TenantId.Should().Be(outerTenant);

            using (VKAmbientExecutionContext.BeginScope(innerTenant))
            {
                VKAmbientExecutionContext.Current!.TenantId.Should().Be(innerTenant);
            }

            VKAmbientExecutionContext.Current!.TenantId.Should().Be(outerTenant);
        }

        VKAmbientExecutionContext.HasContext.Should().BeFalse();
    }

    [Fact]
    public async Task BeginScope_PreservesAcrossAsyncFlows()
    {
        // Arrange
        var tenantId = VKTenantId.Default;

        // Act & Assert
        using (VKAmbientExecutionContext.BeginScope(tenantId))
        {
            await Task.Yield();
            VKAmbientExecutionContext.Current!.TenantId.Should().Be(tenantId);

            await Task.Run(() =>
            {
                VKAmbientExecutionContext.Current!.TenantId.Should().Be(tenantId);
            });
        }
    }

    [Fact]
    public void BeginScope_ProtocolInstances_PushesCoordinates()
    {
        // Arrange
        var tenantCoord = new TestTenantCoordinate(VKTenantId.Default);
        var userCoord = new TestUserCoordinate(VKUserId.System);

        // Act & Assert
        using (VKAmbientExecutionContext.BeginScope(tenantCoord, userCoord))
        {
            VKAmbientExecutionContext.Current!.Tenant.Should().Be(tenantCoord);
            VKAmbientExecutionContext.Current.User.Should().Be(userCoord);
        }

        VKAmbientExecutionContext.HasContext.Should().BeFalse();
    }
}
