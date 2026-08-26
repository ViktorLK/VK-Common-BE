namespace VK.Blocks.Core.Context.Internal;

/// <summary>
/// Minimal execution coordinate holding UserId (Subject dimension).
/// Follows AP.01, AP.03.
/// </summary>
internal sealed record DefaultUserCoordinate(VKUserId UserId) : IVKUserCoordinate;
