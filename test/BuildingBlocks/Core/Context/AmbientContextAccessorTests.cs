using VK.Blocks.Core;
using VK.Blocks.Core.Context.Internal;

namespace VK.Blocks.Core.UnitTests.Context;

public sealed class AmbientContextAccessorTests
{
    private readonly AmbientContextAccessor _accessor = new();

    [Fact]
    public void CurrentProperties_WhenNoAmbientContext_ReturnsNull()
    {
        // Act & Assert
        _accessor.CurrentContext.Should().BeNull();
        _accessor.CurrentTenantCoordinate.Should().BeNull();
        _accessor.CurrentUserCoordinate.Should().BeNull();
    }

    [Fact]
    public void CurrentProperties_WhenAmbientContextSet_ReturnsCoordinates()
    {
        // Arrange
        var tenantId = VKTenantId.Default;
        var userId = VKUserId.System;

        // Act & Assert
        using (_accessor.BeginScope(tenantId, userId))
        {
            _accessor.CurrentContext.Should().NotBeNull();
            _accessor.CurrentTenantCoordinate.Should().NotBeNull();
            _accessor.CurrentTenantCoordinate!.TenantId.Should().Be(tenantId);
            _accessor.CurrentUserCoordinate.Should().NotBeNull();
            _accessor.CurrentUserCoordinate!.UserId.Should().Be(userId);
        }
    }

    [Fact]
    public void ExplicitInterface_TenantId_WhenNoContext_ThrowsInvalidOperationException()
    {
        // Arrange
        var tenantCoord = (IVKTenantCoordinate)_accessor;

        // Act & Assert
        var act = () => tenantCoord.TenantId;
        act.Should().Throw<VKContextException>()
            .WithMessage("*active ambient tenant coordinate*");
    }

    [Fact]
    public void ExplicitInterface_UserId_WhenNoContext_ThrowsInvalidOperationException()
    {
        // Arrange
        var userCoord = (IVKUserCoordinate)_accessor;

        // Act & Assert
        var act = () => userCoord.UserId;
        act.Should().Throw<VKContextException>()
            .WithMessage("*active ambient user coordinate*");
    }

    [Fact]
    public void ExtensionMethods_TryGetTenantId_ExtractsCorrectly()
    {
        // 1. Without context
        _accessor.TryGetTenantId(out var emptyTenantId).Should().BeFalse();
        emptyTenantId.Should().Be(default(VKTenantId));

        // 2. With context
        using (_accessor.BeginScope(VKTenantId.Default))
        {
            _accessor.TryGetTenantId(out var activeTenantId).Should().BeTrue();
            activeTenantId.Should().Be(VKTenantId.Default);
        }
    }

    [Fact]
    public void ExtensionMethods_TryGetUserId_ExtractsCorrectly()
    {
        // 1. Without context
        _accessor.TryGetUserId(out var emptyUserId).Should().BeFalse();
        emptyUserId.Should().Be(default(VKUserId));

        // 2. With context
        using (_accessor.BeginScope(VKUserId.System))
        {
            _accessor.TryGetUserId(out var activeUserId).Should().BeTrue();
            activeUserId.Should().Be(VKUserId.System);
        }
    }
}
