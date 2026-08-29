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

/// <summary>
/// Defines a strongly-typed validator for a specific model type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the model to validate.</typeparam>
public interface IVKValidator<in T> : IVKValidator
{
    /// <summary>
    /// Validates the specified model asynchronously.
    /// </summary>
    Task<VKValidationResult> ValidateAsync(T model, CancellationToken ct = default);

    bool IVKValidator.CanValidate(object model) => model is T;

    Task<VKValidationResult> IVKValidator.ValidateAsync(object model, CancellationToken ct)
    {
        return model is T typedModel
            ? ValidateAsync(typedModel, ct)
            : Task.FromResult(VKValidationResult.Failure(string.Empty, $"Expected model of type {typeof(T).Name}, but received {model?.GetType().Name ?? "null"}."));
    }
}

