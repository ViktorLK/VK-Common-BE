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
    /// Represents the default tenant sentinel value for single-tenant or default contexts.
    /// Replaces nullable <see cref="VKTenantId"/> to prevent null pointer ambiguity.
    /// </summary>
    public static readonly VKTenantId Default = new(Guid.Empty);

    /// <summary>
    /// Attempts to create a <see cref="VKTenantId"/> from a nullable string.
    /// Returns <see cref="Default"/> if the input is null, whitespace, or invalid.
    /// </summary>
    public static VKTenantId FromNullable(string? value) =>
        string.IsNullOrWhiteSpace(value) || !Guid.TryParse(value, out Guid guid) ? Default : new VKTenantId(guid);
}
