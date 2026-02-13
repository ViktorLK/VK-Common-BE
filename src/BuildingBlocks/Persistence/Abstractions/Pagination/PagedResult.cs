using System.Collections.Generic;

namespace VK.Blocks.Persistence.Abstractions.Pagination;

/// <summary>
/// pagination result.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public class PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; } = []; public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
    public bool IsFirstPage => PageNumber == 1;
    public bool IsLastPage => PageNumber >= TotalPages;
}
