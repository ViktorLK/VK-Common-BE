using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Validation;

/// <summary>
/// Defines a pipeline for executing multiple validators.
/// </summary>
public interface IVKValidationPipeline
{
    /// <summary>
    /// Validates the specified model across all applicable validators.
    /// </summary>
    Task<VKValidationResult> ValidateAsync(object model, CancellationToken ct = default);

    /// <summary>
    /// Validates the specified model across all applicable validators for a specific validation group.
    /// </summary>
    Task<VKValidationResult> ValidateAsync(object model, string? group, CancellationToken ct = default)
        => ValidateAsync(model, ct);
}
