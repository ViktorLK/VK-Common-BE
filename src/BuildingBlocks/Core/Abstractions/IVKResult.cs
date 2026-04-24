namespace VK.Blocks.Core;

/// <summary>
/// Represents the result of an operation.
/// </summary>
public interface IVKResult
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    bool IsFailure { get; }

    /// <summary>
    /// Gets the primary error associated with the result.
    /// </summary>
    VKError FirstError { get; }

    /// <summary>
    /// Gets the errors associated with the result.
    /// </summary>
    VKError[] Errors { get; }
}
