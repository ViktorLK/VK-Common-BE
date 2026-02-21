namespace VK.Blocks.Validation.Constants;

/// <summary>
/// ページネーション関連の定数
/// </summary>
public static class PaginationConstants
{
    /// <summary>
    /// パフォーマンスガード
    /// </summary>
    public static class PerformanceGuard
    {
        /// <summary>
        /// オフセットページネーションの最大制限
        /// </summary>
        public const int MaxOffsetLimit = 10000;

        /// <summary>
        /// カーソルページネーションの最大制限
        /// </summary>
        public const int MaxCursorLimit = 10000;

        /// <summary>
        /// ページサイズ
        /// </summary>
        public const int MaxPageSize = 1000;
    }

    /// <summary>
    /// エラーメッセージ
    /// </summary>
    public static class ErrorMessages
    {
        /// <summary>
        /// ページ番号は 0 より大きい必要があります
        /// </summary>
        public const string PageNumberMustBePositive = "ページ番号は 0 より大きい必要があります";

        /// <summary>
        /// ページあたりの件数は 0 より大きい必要があります
        /// </summary>
        public const string PageSizeMustBePositive = "ページあたりの件数は 0 より大きい必要があります";

        /// <summary>
        /// オフセットページネーションの最大制限
        /// </summary>
        public const string OverOffsetLimit = "オフセットページネーションの最大制限を超えています";

        /// <summary>
        /// ページあたりの件数が最大制限を超えています
        /// </summary>
        public const string OverPageSize = "ページあたりの件数が最大制限を超えています";
    }
}
