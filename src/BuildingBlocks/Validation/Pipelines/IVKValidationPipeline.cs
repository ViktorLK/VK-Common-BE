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
}
