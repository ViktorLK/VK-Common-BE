using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Ambient execution context extension methods for Identity module.
/// Follows AP.01, AP.03, AP.06.
/// </summary>
public static class VKIdentityAmbientExtensions
{
    /// <summary>
    /// Attempts to safely extract the active <see cref="IVKUserContext"/> from the ambient execution context.
    /// </summary>
    /// <param name="accessor">The ambient context accessor.</param>
    /// <param name="userContext">When this method returns, contains the user context if resolved; otherwise, null.</param>
    /// <returns><c>true</c> if a rich user context is present in the ambient flow; otherwise, <c>false</c>.</returns>
    public static bool TryGetUserContext(
        this IVKAmbientContextAccessor accessor,
        [NotNullWhen(true)] out IVKUserContext? userContext)
    {
        VKGuard.NotNull(accessor);

        if (accessor.CurrentUserCoordinate is IVKUserContext richContext)
        {
            userContext = richContext;
            return true;
        }

        userContext = null;
        return false;
    }
}
