using System.Collections.Generic;

namespace VK.Blocks.Persistence.Abstractions.Pagination;

/// <summary>
/// Cursor-based pagination result.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public class CursorPagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public string? NextCursor { get; init; }
    public string? PreviousCursor { get; init; }
    public bool HasNextPage { get; init; }
    public bool HasPreviousPage { get; init; }
    public int PageSize { get; init; }
}
public enum CursorDirection
{
    Forward,

    Backward
}
