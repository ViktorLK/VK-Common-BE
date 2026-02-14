

namespace VK.Blocks.Persistence.Abstractions.Pagination;

/// <summary>
/// Represents the result of an offset-based pagination operation.
/// </summary>
/// <typeparam name="T">The type of the elements in the page.</typeparam>
public class PagedResult<T>
{
    #region Properties

    /// <summary>
    /// Gets the collection of items in the current page.
    /// </summary>
    public required IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>
    /// Gets the current page number (1-based).
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Gets the size of the page.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Gets the total count of items across all pages.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Gets a value indicating whether this is the first page.
    /// </summary>
    public bool IsFirstPage => PageNumber == 1;

    /// <summary>
    /// Gets a value indicating whether this is the last page.
    /// </summary>
    public bool IsLastPage => PageNumber >= TotalPages;

    #endregion
}
