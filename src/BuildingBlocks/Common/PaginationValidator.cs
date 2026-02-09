using VK.Blocks.Common.Constants;

namespace VK.Blocks.Common;

/// <summary>
/// ページネーションパラメータの検証
/// </summary>
public static class PaginationValidator
{
    /// <summary>
    /// オフセットページネーションパラメータを検証
    /// </summary>
    /// <param name="pageNumber">ページ番号（1ベース）</param>
    /// <param name="pageSize">ページあたりの件数</param>
    /// <exception cref="ArgumentOutOfRangeException">パラメータが無効な場合</exception>
    public static void ValidateOffsetPagination(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageNumber),
                PaginationConstants.ErrorMessages.PageNumberMustBePositive);
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                PaginationConstants.ErrorMessages.PageSizeMustBePositive);
        }

        if (pageSize > PaginationConstants.PerformanceGuard.MaxPageSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                PaginationConstants.ErrorMessages.OverPageSize);
        }

        var offset = (pageNumber - 1) * pageSize;
        if (offset > PaginationConstants.PerformanceGuard.MaxOffsetLimit)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageNumber),
                PaginationConstants.ErrorMessages.OverOffsetLimit);
        }
    }

    /// <summary>
    /// カーソルページネーションパラメータを検証
    /// </summary>
    /// <param name="pageSize">ページあたりの件数</param>
    /// <exception cref="ArgumentOutOfRangeException">パラメータが無効な場合</exception>
    public static void ValidateCursorPagination(int pageSize)
    {
        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                PaginationConstants.ErrorMessages.PageSizeMustBePositive);
        }

        if (pageSize > PaginationConstants.PerformanceGuard.MaxCursorLimit)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                PaginationConstants.ErrorMessages.OverPageSize);
        }
    }
}
