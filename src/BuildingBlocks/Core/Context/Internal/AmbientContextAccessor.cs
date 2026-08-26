using System;

namespace VK.Blocks.Core.Context.Internal;

/// <summary>
/// Thread-safe implementation of <see cref="IVKAmbientContextAccessor"/>, <see cref="IVKTenantCoordinate"/>, and <see cref="IVKUserCoordinate"/>
/// backed by the unified <see cref="VKAmbientExecutionContext"/>.
/// Follows AP.01, AP.03, AP.06.
/// </summary>
internal sealed class AmbientContextAccessor : IVKAmbientContextAccessor, IVKTenantCoordinate, IVKUserCoordinate
{
    /// <inheritdoc />
    public IVKTenantCoordinate? CurrentTenantCoordinate =>
        VKAmbientExecutionContext.Current?.Tenant;

    /// <inheritdoc />
    public IVKUserCoordinate? CurrentUserCoordinate =>
        VKAmbientExecutionContext.Current?.User;

    /// <inheritdoc />
    public VKExecutionContext? CurrentContext => VKAmbientExecutionContext.Current;

    /// <inheritdoc />
    VKTenantId IVKTenantCoordinate.TenantId =>
        CurrentTenantCoordinate?.TenantId
        ?? throw new InvalidOperationException("Execution requires an active ambient tenant coordinate, but none is set in the active async context.");

    /// <inheritdoc />
    VKUserId IVKUserCoordinate.UserId =>
        CurrentUserCoordinate?.UserId
        ?? throw new InvalidOperationException("Execution requires an active ambient user coordinate, but none is set in the active async context.");

    /// <inheritdoc />
    public IDisposable BeginScope(VKTenantId tenantId) =>
        VKAmbientExecutionContext.BeginScope(tenantId);

    /// <inheritdoc />
    public IDisposable BeginScope(VKUserId userId) =>
        VKAmbientExecutionContext.BeginScope(userId);

    /// <inheritdoc />
    public IDisposable BeginScope(VKTenantId tenantId, VKUserId userId) =>
        VKAmbientExecutionContext.BeginScope(tenantId, userId);

    /// <inheritdoc />
    public IDisposable BeginScope(IVKTenantCoordinate coordinate) =>
        VKAmbientExecutionContext.BeginScope(coordinate);

    /// <inheritdoc />
    public IDisposable BeginScope(IVKUserCoordinate coordinate) =>
        VKAmbientExecutionContext.BeginScope(coordinate);

    /// <inheritdoc />
    public IDisposable BeginScope(IVKTenantCoordinate tenant, IVKUserCoordinate user) =>
        VKAmbientExecutionContext.BeginScope(tenant, user);

    /// <inheritdoc />
    public IDisposable BeginScope(VKExecutionContext context) =>
        VKAmbientExecutionContext.BeginScope(context);
}
