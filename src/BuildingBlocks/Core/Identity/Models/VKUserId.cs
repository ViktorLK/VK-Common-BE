using System;

namespace VK.Blocks.Core;

/// <summary>
/// Strongly-typed identifier for a user entity (CS.06) (AP.01).
/// Encapsulates user identity to prevent primitive obsession.
/// </summary>
[VKStronglyTypedId]
public partial record struct VKUserId
{
    /// <summary>
    /// Represents the anonymous / unauthenticated default user sentinel value.
    /// Used in non-multi-user or unauthenticated guest contexts instead of null.
    /// </summary>
    public static readonly VKUserId Anonymous = new(Guid.Empty);

    /// <summary>
    /// Attempts to create a <see cref="VKUserId"/> from a nullable string.
    /// Returns <see cref="Anonymous"/> if input is null or whitespace.
    /// </summary>
    public static VKUserId FromNullable(string? value) =>
        string.IsNullOrWhiteSpace(value) || !Guid.TryParse(value, out Guid guid) ? Anonymous : new VKUserId(guid);
}
