using System.Collections;

namespace VK.Blocks.Core;

/// <summary>
/// Defines a non-generic contract for paged results to allow covariant handling.
/// </summary>
public interface IVKPagedResult
{
    /// <summary>
    /// Gets the collection of items in the current page.
    /// </summary>
    IEnumerable Items { get; }

    /// <summary>
    /// Gets the current page number (1-based).
    /// </summary>
    int PageNumber { get; }

    /// <summary>
    /// Gets the size of the page.
    /// </summary>
    int PageSize { get; }

    /// <summary>
    /// Gets the total count of items across all pages.
    /// </summary>
    int TotalCount { get; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    int TotalPages { get; }
}
