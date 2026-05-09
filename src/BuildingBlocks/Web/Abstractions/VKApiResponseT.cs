namespace VK.Blocks.Web;

/// <summary>
/// A standardized envelope for all API responses in VK.Blocks.
/// Complies with AP.01 (Modern C#) using sealed records.
/// </summary>
/// <typeparam name="T">The type of the data payload.</typeparam>
public sealed record VKApiResponse<T>
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// The data payload. Null if the operation failed.
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Detailed error information if the operation failed.
    /// </summary>
    public VKWebProblemDetails? VKError { get; init; }
}

