using VK.Blocks.Core;

namespace VK.Blocks.Validation;

/// <summary>
/// Validator for pagination parameters.
/// </summary>
public static class VKPaginationValidator
{
    /// <summary>
    /// Validates offset-based pagination parameters.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>The validation result.</returns>
    public static VKResult ValidateOffsetPagination(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
        {
            return VKResult.Failure(VKPaginationErrors.InvalidPageNumber);
        }

        if (pageSize < 1)
        {
            return VKResult.Failure(VKPaginationErrors.InvalidPageSize);
        }

        if (pageSize > VKPaginationConstants.MaxPageSize)
        {
            return VKResult.Failure(VKPaginationErrors.OverPageSize);
        }

        var offset = (long)(pageNumber - 1) * pageSize;
        if (offset > VKPaginationConstants.MaxOffsetLimit)
        {
            return VKResult.Failure(VKPaginationErrors.OverOffsetLimit);
        }

        return VKResult.Success();
    }

    /// <summary>
    /// Validates cursor-based pagination parameters.
    /// </summary>
    /// <param name="pageSize">The number of items to retrieve.</param>
    /// <returns>The validation result.</returns>
    public static VKResult ValidateCursorPagination(int pageSize)
    {
        if (pageSize < 1)
        {
            return VKResult.Failure(VKPaginationErrors.InvalidPageSize);
        }

        if (pageSize > VKPaginationConstants.MaxCursorLimit)
        {
            return VKResult.Failure(VKPaginationErrors.OverPageSize);
        }

        return VKResult.Success();
    }
}
