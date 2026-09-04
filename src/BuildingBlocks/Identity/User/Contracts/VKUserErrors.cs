using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain errors associated with user lifecycle and credential operations.
/// </summary>
public static class VKUserErrors
{
    public static readonly VKError EmailRequired = VKError.Validation(
        "User.EmailRequired", "Email address is required.");

    public static readonly VKError EmailInvalidFormat = VKError.Validation(
        "User.EmailInvalidFormat", "Email address format is invalid.");

    public static readonly VKError UserNotFound = VKError.NotFound(
        "User.NotFound", "The requested user was not found.");

    public static readonly VKError UserAlreadyExists = VKError.Conflict(
        "User.AlreadyExists", "A user with the specified email already exists.");

    public static readonly VKError UserDisabled = VKError.Forbidden(
        "User.Disabled", "User account has been disabled.");
}
