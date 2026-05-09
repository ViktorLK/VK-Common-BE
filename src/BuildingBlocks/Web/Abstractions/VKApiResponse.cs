namespace VK.Blocks.Web;

/// <summary>
/// Static factory for creating <see cref="VKApiResponse{T}"/>.
/// Solves CA1000 by moving static members out of the generic type.
/// </summary>
public static class VKApiResponse
{
    /// <summary>
    /// Creates a successful response with the specified data.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="data">The response data.</param>
    /// <returns>A successful <see cref="VKApiResponse{T}"/>.</returns>
    public static VKApiResponse<T> Success<T>(T data) => new()
    {
        Success = true,
        Data = data
    };

    /// <summary>
    /// Creates a failed response with the specified problem details.
    /// </summary>
    /// <typeparam name="T">The type of the data (optional).</typeparam>
    /// <param name="problemDetails">The error details.</param>
    /// <returns>A failed <see cref="VKApiResponse{T}"/>.</returns>
    public static VKApiResponse<T> Failure<T>(VKWebProblemDetails problemDetails) => new()
    {
        Success = false,
        VKError = problemDetails
    };
}
