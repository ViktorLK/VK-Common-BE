using VK.Blocks.Core;

namespace VK.Blocks.Validation;

/// <summary>
/// Error definitions for pagination validation.
/// </summary>
public static class VKPaginationErrors
{
    /// <summary>Error for invalid page number.</summary>
    public static readonly VKError InvalidPageNumber = new("Pagination.InvalidPageNumber", VKPaginationConstants.PageNumberMustBePositive, VKErrorType.Validation);

    /// <summary>Error for invalid page size.</summary>
    public static readonly VKError InvalidPageSize = new("Pagination.InvalidPageSize", VKPaginationConstants.PageSizeMustBePositive, VKErrorType.Validation);

    /// <summary>Error for page size exceeding limit.</summary>
    public static readonly VKError OverPageSize = new("Pagination.OverPageSize", VKPaginationConstants.OverPageSize, VKErrorType.Validation);

    /// <summary>Error for offset exceeding limit.</summary>
    public static readonly VKError OverOffsetLimit = new("Pagination.OverOffsetLimit", VKPaginationConstants.OverOffsetLimit, VKErrorType.Validation);
}
