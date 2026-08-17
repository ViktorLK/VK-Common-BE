using System.Linq;
using VK.Blocks.Core;

namespace VK.Blocks.Web;

/// <summary>
/// Static factory for creating <see cref="VKPagedResponse{T}"/>.
/// Solves CA1000 by moving static members out of the generic type.
/// </summary>
public static class VKPagedResponse
{
    /// <summary>
    /// Creates a successful <see cref="VKPagedResponse{T}"/> from the specified paged result.
    /// </summary>
    /// <typeparam name="T">The type of items.</typeparam>
    /// <param name="pagedResult">The paged result from the application layer.</param>
    /// <returns>A successful <see cref="VKPagedResponse{T}"/>.</returns>
    public static VKPagedResponse<T> Success<T>(VKPagedResult<T> pagedResult) => new()
    {
        Success = true,
        Items = pagedResult.Items,
        PageNumber = pagedResult.PageNumber,
        PageSize = pagedResult.PageSize,
        TotalCount = pagedResult.TotalCount,
        TotalPages = pagedResult.TotalPages
    };

    /// <summary>
    /// Creates a successful <see cref="VKPagedResponse{object}"/> from a non-generic <see cref="IVKPagedResult"/>.
    /// </summary>
    /// <param name="pagedResult">The paged result.</param>
    /// <returns>A successful <see cref="VKPagedResponse{object}"/>.</returns>
    public static VKPagedResponse<object> Success(IVKPagedResult pagedResult) => new()
    {
        Success = true,
        Items = pagedResult.Items.Cast<object>(),
        PageNumber = pagedResult.PageNumber,
        PageSize = pagedResult.PageSize,
        TotalCount = pagedResult.TotalCount,
        TotalPages = pagedResult.TotalPages
    };

    /// <summary>
    /// Creates a failed <see cref="VKPagedResponse{T}"/> with the specified problem details.
    /// </summary>
    /// <typeparam name="T">The type of items.</typeparam>
    /// <param name="problemDetails">The problem details describing the failure.</param>
    /// <returns>A failed <see cref="VKPagedResponse{T}"/>.</returns>
    public static VKPagedResponse<T> Failure<T>(VKWebProblemDetails problemDetails) => new()
    {
        Success = false,
        Items = [],
        PageNumber = 0,
        PageSize = 0,
        TotalCount = 0,
        TotalPages = 0,
        VKError = problemDetails
    };
}
