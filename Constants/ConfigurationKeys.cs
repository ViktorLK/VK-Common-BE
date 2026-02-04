namespace VK_Common_BE.Constants
{
    /// <summary>
    /// 設定キー定数
    /// </summary>
    public static class ConfigurationKeys
    {
        /// <summary>
        /// 接続文字列
        /// </summary>
        public static class ConnectionStrings
        {
            /// <summary>
            /// デフォルト接続文字列
            /// </summary>
            public const string Default = "DefaultConnection";
        }

        /// <summary>
        /// Azure B2C 設定セクション
        /// </summary>
        public static class AzureB2C
        {
            /// <summary>
            /// 設定セクションパス
            /// </summary>
            public const string SectionName = "AzureB2C";

            /// <summary>
            /// Authority設定キー
            /// </summary>
            public const string Authority = "Authority";

            /// <summary>
            /// ClientId設定キー
            /// </summary>
            public const string ClientId = "ClientId";

            /// <summary>
            /// Instance設定キー
            /// </summary>
            public const string Instance = "Instance";

            /// <summary>
            /// Domain設定キー
            /// </summary>
            public const string Domain = "Domain";

            /// <summary>
            /// TenantId設定キー
            /// </summary>
            public const string TenantId = "TenantId";

            /// <summary>
            /// SignUpSignInPolicyId設定キー
            /// </summary>
            public const string SignUpSignInPolicyId = "SignUpSignInPolicyId";
        }

        /// <summary>
        /// 認証設定セクション
        /// </summary>
        public static class Authentication
        {
            /// <summary>
            /// APIキー設定セクションパス
            /// </summary>
            public const string ApiKeys = "Authentication:ApiKeys";
        }

        /// <summary>
        /// CORS 設定セクション
        /// </summary>
        public static class Cors
        {
            /// <summary>
            /// 設定セクションパス
            /// </summary>
            public const string SectionName = "Cors";

            /// <summary>
            /// 許可するオリジン設定キー
            /// </summary>
            public const string AllowedOrigins = "AllowedOrigins";
        }
    }
}
