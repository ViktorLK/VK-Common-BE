namespace VK.Blocks.Persistence.Core.Pagination;

/// <summary>
/// Represents the result of a cursor-based pagination operation.
/// </summary>
/// <typeparam name="T">The type of the elements in the page.</typeparam>
public class CursorPagedResult<T>
{
    #region Properties

    /// <summary>
    /// Gets the collection of items in the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>
    /// Gets the cursor for the next page, or <c>null</c> if there are no more pages.
    /// </summary>
    public string? NextCursor { get; init; }

    /// <summary>
    /// Gets the cursor for the previous page, or <c>null</c> if there are no previous pages.
    /// </summary>
    public string? PreviousCursor { get; init; }

    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    public bool HasNextPage { get; init; }

    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage { get; init; }

    /// <summary>
    /// Gets the size of the page.
    /// </summary>
    public int PageSize { get; init; }

    #endregion
}

public enum CursorDirection
{
    Forward,

    Backward
}
