using VK.Blocks.Core.Results;
using VK.Blocks.Validation.Constants;

namespace VK.Blocks.Validation;

/// <summary>
/// ページネーションパラメータの検証
/// </summary>
public static class PaginationValidator
{
    /// <summary>
    /// ページネーションバリデーションに関連するエラー定義
    /// </summary>
    public static class Errors
    {
        /// <summary>ページ番号が無効な場合のエラー</summary>
        public static readonly Error InvalidPageNumber = new("Pagination.InvalidPageNumber", PaginationConstants.ErrorMessages.PageNumberMustBePositive, ErrorType.Validation);

        /// <summary>ページサイズが負またはゼロの場合のエラー</summary>
        public static readonly Error InvalidPageSize = new("Pagination.InvalidPageSize", PaginationConstants.ErrorMessages.PageSizeMustBePositive, ErrorType.Validation);

        /// <summary>ページサイズが最大制限を超えている場合のエラー</summary>
        public static readonly Error OverPageSize = new("Pagination.OverPageSize", PaginationConstants.ErrorMessages.OverPageSize, ErrorType.Validation);

        /// <summary>オフセット制限を超えている場合のエラー</summary>
        public static readonly Error OverOffsetLimit = new("Pagination.OverOffsetLimit", PaginationConstants.ErrorMessages.OverOffsetLimit, ErrorType.Validation);
    }

    /// <summary>
    /// オフセットページネーションパラメータを検証
    /// </summary>
    /// <param name="pageNumber">ページ番号（1ベース）</param>
    /// <param name="pageSize">ページあたりの件数</param>
    /// <returns>検証結果</returns>
    public static Result ValidateOffsetPagination(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
        {
            return Result.Failure(Errors.InvalidPageNumber);
        }

        if (pageSize < 1)
        {
            return Result.Failure(Errors.InvalidPageSize);
        }

        if (pageSize > PaginationConstants.PerformanceGuard.MaxPageSize)
        {
            return Result.Failure(Errors.OverPageSize);
        }

        var offset = (long)(pageNumber - 1) * pageSize;
        if (offset > PaginationConstants.PerformanceGuard.MaxOffsetLimit)
        {
            return Result.Failure(Errors.OverOffsetLimit);
        }

        return Result.Success();
    }

    /// <summary>
    /// カーソルページネーションパラメータを検証
    /// </summary>
    /// <param name="pageSize">ページあたりの件数</param>
    /// <returns>検証結果</returns>
    public static Result ValidateCursorPagination(int pageSize)
    {
        if (pageSize < 1)
        {
            return Result.Failure(Errors.InvalidPageSize);
        }

        if (pageSize > PaginationConstants.PerformanceGuard.MaxCursorLimit)
        {
            return Result.Failure(Errors.OverPageSize);
        }

        return Result.Success();
    }
}
