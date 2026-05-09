using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Validation;

/// <summary>
/// Defines a validator for a specific model or object.
/// </summary>
public interface IVKValidator
{
    /// <summary>
    /// Determines whether this validator can validate the specified model.
    /// </summary>
    bool CanValidate(object model);

    /// <summary>
    /// Validates the specified model asynchronously.
    /// </summary>
    Task<VKValidationResult> ValidateAsync(object model, CancellationToken ct = default);
}
