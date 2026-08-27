using System.Diagnostics.CodeAnalysis;

namespace VK.Blocks.Core;

/// <summary>
/// Extension methods providing ergonomic safe extraction of coordinates from <see cref="IVKAmbientContextAccessor"/> and <see cref="VKExecutionContext"/>.
/// Follows AP.01, AP.03, and AP.06.
/// </summary>
public static class VKAmbientContextExtensions
{
    /// <summary>
    /// Attempts to safely extract the active strongly-typed tenant identifier from the ambient context accessor.
    /// </summary>
    /// <param name="accessor">The ambient context accessor instance.</param>
    /// <param name="tenantId">When this method returns, contains the tenant identifier if present; otherwise, the default value.</param>
    /// <returns><c>true</c> if a tenant coordinate is present in the ambient context; otherwise, <c>false</c>.</returns>
    public static bool TryGetTenantId(
        this IVKAmbientContextAccessor accessor,
        [NotNullWhen(true)] out VKTenantId tenantId)
    {
        VKGuard.NotNull(accessor);

        var coordinate = accessor.CurrentTenantCoordinate;
        if (coordinate is not null)
        {
            tenantId = coordinate.TenantId;
            return true;
        }

        tenantId = default;
        return false;
    }

    /// <summary>
    /// Attempts to safely extract the active strongly-typed user identifier from the ambient context accessor.
    /// </summary>
    /// <param name="accessor">The ambient context accessor instance.</param>
    /// <param name="userId">When this method returns, contains the user identifier if present; otherwise, the default value.</param>
    /// <returns><c>true</c> if a user coordinate is present in the ambient context; otherwise, <c>false</c>.</returns>
    public static bool TryGetUserId(
        this IVKAmbientContextAccessor accessor,
        [NotNullWhen(true)] out VKUserId userId)
    {
        VKGuard.NotNull(accessor);

        var coordinate = accessor.CurrentUserCoordinate;
        if (coordinate is not null)
        {
            userId = coordinate.UserId;
            return true;
        }

        userId = default;
        return false;
    }
}
