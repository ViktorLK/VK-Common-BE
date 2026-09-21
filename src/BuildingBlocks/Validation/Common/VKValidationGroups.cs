namespace VK.Blocks.Validation;

/// <summary>
/// Common standardized validation group names for scenario-scoped validation (e.g. Create vs Update).
/// </summary>
public static class VKValidationGroups
{
    /// <summary>
    /// Rules applicable when creating a new entity or resource.
    /// </summary>
    public const string Create = "Create";

    /// <summary>
    /// Rules applicable when updating an existing entity or resource.
    /// </summary>
    public const string Update = "Update";

    /// <summary>
    /// Rules applicable when deleting an entity or resource.
    /// </summary>
    public const string Delete = "Delete";

    /// <summary>
    /// Default validation rules applied across all scenarios.
    /// </summary>
    public const string Default = "Default";
}
