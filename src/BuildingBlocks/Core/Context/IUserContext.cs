namespace VK.Blocks.Core.Context;

/// <summary>
/// Provides access to the current user's identity and authentication state
/// within the application's execution context.
/// </summary>
public interface IUserContext
{
    #region Properties

    /// <summary>
    /// Gets the unique identifier of the current user,
    /// or <c>null</c> if the user is not authenticated.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the display name of the current user,
    /// or <c>null</c> if the user is not authenticated.
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Gets a value indicating whether the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    #endregion
}
