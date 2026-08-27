namespace VK.Blocks.Core;

/// <summary>
/// Minimal execution coordinate providing a strongly-typed UserId coordinate.
/// Represents the fundamental subject (Who) coordinate in the VK.Blocks architecture.
/// Follows AP.01, AP.03.
/// </summary>
public interface IVKUserCoordinate
{
    /// <summary>
    /// Gets the current strongly-typed UserId coordinate.
    /// </summary>
    VKUserId UserId { get; }
}
