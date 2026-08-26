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
    /// Represents the anonymous / unauthenticated default user sentinel value (00000000-0000-0000-0000-000000000002).
    /// Used in non-multi-user or unauthenticated guest contexts instead of Guid.Empty.
    /// </summary>
    public static readonly VKUserId Anonymous = new(Guid.Parse("00000000-0000-0000-0000-000000000002"));

    /// <summary>
    /// Represents the internal system execution user sentinel value (00000000-0000-0000-0000-000000000003).
    /// Used for background jobs, system tasks, and non-user triggered executions.
    /// </summary>
    public static readonly VKUserId System = new(Guid.Parse("00000000-0000-0000-0000-000000000003"));

    /// <summary>
    /// Attempts to create a <see cref="VKUserId"/> from a nullable string.
    /// Returns <see cref="Anonymous"/> if input is null or whitespace.
    /// </summary>
    public static VKUserId FromNullable(string? value) =>
        string.IsNullOrWhiteSpace(value) || !Guid.TryParse(value, out Guid guid) ? Anonymous : new VKUserId(guid);
}
