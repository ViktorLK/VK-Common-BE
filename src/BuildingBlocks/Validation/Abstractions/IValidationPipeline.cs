using VK.Blocks.Validation.Abstractions.Contracts;

namespace VK.Blocks.Validation.Abstractions;

/// <summary>
/// Defines a pipeline for executing multiple validators.
/// </summary>
public interface IValidationPipeline
{
    /// <summary>
    /// Validates the specified model across all applicable validators.
    /// </summary>
    Task<ValidationResult> ValidateAsync(object model, CancellationToken ct = default);
}
