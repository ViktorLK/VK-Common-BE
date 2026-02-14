namespace VK.Blocks.Persistence.Abstractions.Entities;

/// <summary>
/// Defines a provider for accessing the current user's information.
/// </summary>
public interface ICurrentUser
{
    #region Properties

    /// <summary>
    /// Gets the unique identifier of the current user, or <c>null</c> if not authenticated.
    /// </summary>
    string? Id { get; }

    #endregion
}
