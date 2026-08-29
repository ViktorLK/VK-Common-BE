namespace VK.Blocks.Validation;

/// <summary>
/// Defines the ordering/priority of validator execution within the validation pipeline.
/// </summary>
public interface IVKValidationOrder
{
    /// <summary>
    /// Gets the execution order of the validator. Lower numbers run first.
    /// Default order is 0.
    /// </summary>
    int Order => 0;
}
