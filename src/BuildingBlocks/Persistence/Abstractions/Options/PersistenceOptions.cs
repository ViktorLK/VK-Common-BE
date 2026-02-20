
namespace VK.Blocks.Persistence.Abstractions.Options;

/// <summary>
/// Configuration options for the persistence layer.
/// </summary>
public class PersistenceOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether auditing is enabled.
    /// </summary>
    #region Properties

    /// <summary>
    /// Gets or sets a value indicating whether auditing is enabled.
    /// </summary>
    public bool EnableAuditing { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether soft delete is enabled.
    /// </summary>
    public bool EnableSoftDelete { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether multi-tenancy is enabled.
    /// </summary>
    public bool EnableMultiTenancy { get; set; } = false;

    #endregion
}
