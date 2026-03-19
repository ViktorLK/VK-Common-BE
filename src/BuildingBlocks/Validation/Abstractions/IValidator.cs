using VK.Blocks.Validation.Abstractions.Contracts;

namespace VK.Blocks.Validation.Abstractions;

/// <summary>
/// Defines a validator for a specific model or object.
/// </summary>
public interface IValidator
{
    /// <summary>
    /// Determines whether this validator can validate the specified model.
    /// </summary>
    bool CanValidate(object model);

    /// <summary>
    /// Validates the specified model asynchronously.
    /// </summary>
    Task<ValidationResult> ValidateAsync(object model, CancellationToken ct = default);
}
