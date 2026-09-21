namespace VK.Blocks.Validation;

/// <summary>
/// Allows a model or request to dynamically specify its validation scenario/group.
/// </summary>
public interface IVKGroupValidatable
{
    /// <summary>
    /// Gets the validation group to execute (e.g. <see cref="VKValidationGroups.Create"/>).
    /// </summary>
    string? ValidationGroup { get; }
}
