using System;

namespace VK.Blocks.Core;

/// <summary>
/// Strongly-typed identifier for a tenant (CS.06) (AP.01).
/// Encapsulates tenant identity to prevent primitive obsession.
/// </summary>
[VKStronglyTypedId]
public partial record struct VKTenantId
{
    /// <summary>
    /// Attempts to create a <see cref="VKTenantId"/> from a nullable string.
    /// Returns null if the input is null or whitespace.
    /// </summary>
    public static VKTenantId? FromNullable(string? value) =>
        string.IsNullOrWhiteSpace(value) || !Guid.TryParse(value, out Guid guid) ? null : new VKTenantId(guid);
}
