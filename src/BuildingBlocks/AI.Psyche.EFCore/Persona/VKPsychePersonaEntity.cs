using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore;

/// <summary>
/// Database entity representing a VK AI Persona.
/// Pure persistence model for Psyche IVKPersonaStore. [CS.05] [CS.08]
/// </summary>
[VKPersistEntity(typeof(VKPersonaAnchor), TableName = "VK_AI_Psyche_Persona")]
public sealed class VKPsychePersonaEntity : IVKTenantScoped, IVKFullAuditable
{
    /// <inheritdoc />
    [VKPersistIndex(Group = "Tenant_Name", Order = 1)]
    public VKTenantId? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the unique strongly-typed persona identifier.
    /// </summary>
    [VKPersistKey]
    public required VKPersonaId Id { get; set; }

    /// <summary>
    /// Gets or sets the unique name of the persona within the tenant scope.
    /// </summary>
    [Required]
    [MaxLength(128)]
    [VKPersistIndex(Group = "Tenant_Name", Order = 2)]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the detailed description and role framing for this persona.
    /// </summary>
    [MaxLength(4000)]
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets the persona personality traits and behavioral nuances.
    /// </summary>
    [VKPersistColumn(TypeName = "jsonb")]
    public IReadOnlyDictionary<string, string> Traits { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets arbitrary JSON metadata extensions for runtime customization.
    /// </summary>
    [VKPersistColumn(TypeName = "jsonb")]
    public IReadOnlyDictionary<string, object> Extensions { get; set; } = new Dictionary<string, object>();

    /// <inheritdoc />
    [VKPersistIndex]
    public bool IsDeleted { get; set; }

    /// <inheritdoc />
    public DateTimeOffset CreatedAt { get; set; }

    /// <inheritdoc />
    public VKUserId? CreatedBy { get; set; }

    /// <inheritdoc />
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <inheritdoc />
    public VKUserId? UpdatedBy { get; set; }

    /// <inheritdoc />
    public DateTimeOffset? DeletedAt { get; set; }

    /// <inheritdoc />
    public VKUserId? DeletedBy { get; set; }
}
