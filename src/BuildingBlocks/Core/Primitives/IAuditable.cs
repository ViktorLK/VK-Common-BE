namespace VK.Blocks.Core.Primitives;

/// <summary>
/// Defines the contract for auditable entities that track creation and modification details.
/// </summary>
public interface IAuditable
{
    #region Properties

    /// <summary>Gets or sets the date and time when the entity was created (UTC).</summary>
    DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets the identifier of the user who created the entity.</summary>
    string? CreatedBy { get; set; }

    /// <summary>Gets or sets the date and time when the entity was last updated (UTC).</summary>
    DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>Gets or sets the identifier of the user who last updated the entity.</summary>
    string? UpdatedBy { get; set; }

    #endregion
}
