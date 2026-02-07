namespace VK.Lab.LayeredArchitecture.Constants
{
    /// <summary>
    /// 認証関連の定数
    /// </summary>
    public static class AuthenticationConstants
    {
        /// <summary>
        /// APIキーのリクエスト�EチE��ー吁E
        /// </summary>
        public const string ApiKeyHeaderName = "X-Api-Key";

        /// <summary>
        /// 認証スキーム吁E
        /// </summary>
        public static class Schemes
        {
            /// <summary>
            /// 動的認証スキーム�E�リクエスト�EチE��ーに基づぁE��自動選択！E
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
        /// 認可ポリシー吁E
        /// </summary>
        public static class Policies
        {
            /// <summary>
            /// 管琁E��E��ールが忁E��E
            /// </summary>
            public const string RequireAdminRole = "RequireAdminRole";

            /// <summary>
            /// ユーザーロールが忁E��E��Eserまた�EAdmin�E�E
            /// </summary>
            public const string RequireUserRole = "RequireUserRole";

            /// <summary>
            /// ぁE��れかの認証スキームを許可�E�EPIキーまた�EAzure B2C�E�E
            /// </summary>
            public const string ApiOrB2C = "ApiOrB2C";
        }

        /// <summary>
        /// ロール吁E
        /// </summary>
        public static class Roles
        {
            /// <summary>
            /// 管琁E��E��ール
            /// </summary>
            public const string Admin = "Admin";

            /// <summary>
            /// ユーザーロール
            /// </summary>
            public const string User = "User";
        }

        /// <summary>
        /// クレームタイチE
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
