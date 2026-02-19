namespace VK.Blocks.Core.Results;

/// <summary>
/// Represents the result of an operation.
/// </summary>
public interface IResult
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
    Error Error { get; }

    /// <summary>
    /// Gets the errors associated with the result.
    /// </summary>
    Error[] Errors { get; }
}
