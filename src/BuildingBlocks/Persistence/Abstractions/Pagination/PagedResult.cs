using System.Collections.Generic;

namespace VK.Blocks.Persistence.Abstractions.Pagination;

/// <summary>
/// Cursor-based pagination result.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public class PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public string? NextCursor { get; init; }
    public bool HasNextPage { get; init; }
}
