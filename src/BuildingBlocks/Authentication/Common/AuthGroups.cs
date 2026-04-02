namespace VK.Blocks.Authentication.Common;

/// <summary>
/// Predefined logical authentication groups for multi-strategy policy generation.
/// </summary>
public static class AuthGroups
{
    #region Fields

    /// <summary>
    /// Represents standard interactive users (e.g. JWT-based).
    /// </summary>
    public const string User = "User";

    /// <summary>
    /// Represents non-interactive services or clients (e.g. ApiKey-based).
    /// </summary>
    public const string Service = "Service";

    /// <summary>
    /// Represents internal machine-to-machine or administrative access.
    /// </summary>
    public const string Internal = "Internal";

    #endregion
}
