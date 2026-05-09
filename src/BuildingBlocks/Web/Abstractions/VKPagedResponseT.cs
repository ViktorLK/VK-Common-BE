using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.Web;

/// <summary>
/// A standardized envelope for paged API responses in VK.Blocks.
/// Wraps <see cref="VKPagedResult{T}"/> for the presentation layer.
/// </summary>
/// <typeparam name="T">The type of items in the page.</typeparam>
public sealed record VKPagedResponse<T>
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// The collection of items for the current page.
    /// </summary>
    public required IEnumerable<T> Items { get; init; }

    /// <summary>
    /// The current page number.
    /// </summary>
    public required int PageNumber { get; init; }

    /// <summary>
    /// The size of each page.
    /// </summary>
    public required int PageSize { get; init; }

    /// <summary>
    /// The total number of items across all pages.
    /// </summary>
    public required int TotalCount { get; init; }

    /// <summary>
    /// The total number of pages.
    /// </summary>
    public required int TotalPages { get; init; }

    /// <summary>
    /// Detailed error information if the operation failed.
    /// </summary>
    public VKWebProblemDetails? VKError { get; init; }
}
