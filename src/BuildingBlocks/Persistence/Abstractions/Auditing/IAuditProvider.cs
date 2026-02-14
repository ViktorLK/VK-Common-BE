namespace VK.Blocks.Persistence.Abstractions.Auditing;

/// <summary>
/// Defines a provider for retrieving audit information.
/// </summary>
public interface IAuditProvider
{
    #region Properties

    /// <summary>
    /// Gets the unique identifier of the current user.
    /// </summary>
    string CurrentUserId { get; }

    /// <summary>
    /// Gets the name of the current user.
    /// </summary>
    string CurrentUserName { get; }

    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTimeOffset UtcNow { get; }

    /// <summary>
    /// Gets a value indicating whether the user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    #endregion
}
