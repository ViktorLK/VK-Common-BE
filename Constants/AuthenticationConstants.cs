namespace VK_Common_BE.Constants
{
    /// <summary>
    /// 認証関連の定数
    /// </summary>
    public static class AuthenticationConstants
    {
        /// <summary>
        /// APIキーのリクエストヘッダー名
        /// </summary>
        public const string ApiKeyHeaderName = "X-Api-Key";

        /// <summary>
        /// 認証スキーム名
        /// </summary>
        public static class Schemes
        {
            /// <summary>
            /// 動的認証スキーム（リクエストヘッダーに基づいて自動選択）
            /// </summary>
            public const string Dynamic = "DynamicScheme";

            /// <summary>
            /// APIキー認証スキーム
            /// </summary>
            public const string ApiKey = "ApiKey";

            /// <summary>
            /// Azure B2C JWT認証スキーム
            /// </summary>
            public const string AzureB2C = "AzureB2C";
        }

        /// <summary>
        /// 認可ポリシー名
        /// </summary>
        public static class Policies
        {
            /// <summary>
            /// 管理者ロールが必要
            /// </summary>
            public const string RequireAdminRole = "RequireAdminRole";

            /// <summary>
            /// ユーザーロールが必要（UserまたはAdmin）
            /// </summary>
            public const string RequireUserRole = "RequireUserRole";

            /// <summary>
            /// いずれかの認証スキームを許可（APIキーまたはAzure B2C）
            /// </summary>
            public const string ApiOrB2C = "ApiOrB2C";
        }

        /// <summary>
        /// ロール名
        /// </summary>
        public static class Roles
        {
            /// <summary>
            /// 管理者ロール
            /// </summary>
            public const string Admin = "Admin";

            /// <summary>
            /// ユーザーロール
            /// </summary>
            public const string User = "User";
        }

        /// <summary>
        /// クレームタイプ
        /// </summary>
        public static class Claims
        {
            /// <summary>
            /// APIキーID
            /// </summary>
            public const string ApiKeyId = "ApiKeyId";
        }
    }
}
