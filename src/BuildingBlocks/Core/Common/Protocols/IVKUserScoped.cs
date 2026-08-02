namespace VK.Blocks.Core;

/// <summary>
/// Domain contract for entities and DTOs that are bound to a user security boundary.
/// Supports zero-code global user-level isolation filters and EF Core interceptors.
/// </summary>
public interface IVKUserScoped
{
    /// <summary>
    /// Gets the strongly-typed user identifier.
    /// </summary>
    VKUserId UserId { get; }
}
