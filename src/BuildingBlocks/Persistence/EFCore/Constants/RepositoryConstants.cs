namespace VK.Blocks.Persistence.EFCore.Constants;

/// <summary>
/// リポジトリ層の定数
/// 特徴：可読性・保守性向上のための文字列リテラルの一元管理
/// </summary>
public static class RepositoryConstants
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
        /// カーソルページネーションの最大制限
        /// </summary>
        public const string OverPageSize = "ページあたりの件数が最大制限を超えています";

        /// <summary>
        /// 既にアクティブなトランザクションが存在します
        /// </summary>
        public const string TransactionAlreadyActive = "既にアクティブなトランザクションが存在します";

        /// <summary>
        /// コミット可能なアクティブなトランザクションがありません
        /// </summary>
        public const string NoActiveTransaction = "コミット可能なアクティブなトランザクションがありません";

        /// <summary>
        /// 主キーが見つかりませんでした（エンティティ名を挿入して使用）
        /// </summary>
        public const string PrimaryKeyNotFoundFormat = "エンティティ '{0}' に主キーが見つかりません。FindWithIncludesAsync を使用してください。";
    }

    /// <summary>
    /// Expression パラメータ名
    /// </summary>
    public static class ExpressionParameterNames
    {
        /// <summary>
        /// エンティティパラメータ名
        /// </summary>
        public const string Entity = "e";

        /// <summary>
        /// 汎用アイテムパラメータ名
        /// </summary>
        public const string Item = "x";
    }

    /// <summary>
    /// プロパティ名
    /// </summary>
    public static class PropertyNames
    {
        /// <summary>
        /// ID プロパティ名
        /// </summary>
        public const string Id = "Id";

        /// <summary>
        /// 論理削除フラグプロパティ名
        /// </summary>
        public const string IsDeleted = "IsDeleted";
    }

    /// <summary>
    /// LINQ メソッド名
    /// </summary>
    public static class LinqMethodNames
    {
        /// <summary>
        /// 昇順ソートメソッド名
        /// </summary>
        public const string OrderBy = "OrderBy";

        /// <summary>
        /// 降順ソートメソッド名
        /// </summary>
        public const string OrderByDescending = "OrderByDescending";
    }
}
