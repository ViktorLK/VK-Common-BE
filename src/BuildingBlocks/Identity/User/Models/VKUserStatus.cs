namespace VK.Blocks.Identity;

/// <summary>
/// Domain-level lifecycle and operation status of a user entity.
/// </summary>
public enum VKUserStatus : byte
{
    /// <summary>
    /// User account is pending email confirmation.
    /// </summary>
    PendingVerification = 0,

    /// <summary>
    /// User account is active and in good standing.
    /// </summary>
    Active = 1,

    /// <summary>
    /// User account is permanently disabled by administrator.
    /// </summary>
    Disabled = 2
}
