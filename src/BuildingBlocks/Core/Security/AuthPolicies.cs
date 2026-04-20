namespace VK.Blocks.Core.Security;

/// <summary>
/// Predefined logical authentication groups and policy names used across the VK.Blocks framework.
/// Moved to Core to allow decoupling between Authentication and Authorization modules.
/// </summary>
public static class AuthPolicies
{
    /// <summary>
    /// Represents standard interactive users (e.g. JWT-based).
    /// </summary>
    public const string GroupUser = "VK.Group.User";

    /// <summary>
    /// Represents non-interactive services or clients (e.g. ApiKey-based).
    /// </summary>
    public const string GroupService = "VK.Group.Service";

    /// <summary>
    /// Represents internal machine-to-machine or administrative access.
    /// </summary>
    public const string GroupInternal = "VK.Group.Internal";

    /// <summary>
    /// Individual policy for JWT-based authentication.
    /// </summary>
    public const string Jwt = "VK.Jwt";

    /// <summary>
    /// Individual policy for API key-based authentication.
    /// </summary>
    public const string ApiKey = "VK.ApiKey";
}
