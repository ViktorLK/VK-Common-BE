namespace VK_Common_BE.Constants
{
    /// <summary>
    /// メッセージ定数
    /// </summary>
    public static class MessageConstants
    {
        /// <summary>
        /// エラーメッセージ
        /// </summary>
        public static class Errors
        {
            /// <summary>
            /// 製品が見つからない場合のメッセージテンプレート
            /// </summary>
            public const string ProductNotFound = "Product with ID {0} not found";

            /// <summary>
            /// 製品IDが一致しない場合のメッセージ
            /// </summary>
            public const string ProductIdMismatch = "Product ID mismatch";

            /// <summary>
            /// 無効なAPIキーのメッセージ
            /// </summary>
            public const string InvalidApiKey = "Invalid API Key";

            /// <summary>
            /// 有効なAPIキーが設定されていない場合のメッセージ
            /// </summary>
            public const string NoValidApiKeys = "No valid API Keys configured";

            /// <summary>
            /// IPがホワイトリストに含まれていない場合のメッセージ
            /// </summary>
            public const string IpNotWhitelisted = "IP address not whitelisted";

            /// <summary>
            /// APIキーは読み取り専用のメッセージ
            /// </summary>
            public const string ApiKeyReadOnlyAccess = "API Key authentication only allows GET requests. Please use Azure B2C authentication for write operations.";
        }

        /// <summary>
        /// ログメッセージ
        /// </summary>
        public static class Logging
        {
            /// <summary>
            /// Azure B2C認証失敗メッセージテンプレート
            /// </summary>
            public const string AzureB2CAuthFailed = "Azure B2C authentication failed: {0}";

            /// <summary>
            /// Azure B2Cトークン検証成功メッセージ
            /// </summary>
            public const string AzureB2CTokenValidated = "Azure B2C token validated successfully";
        }
    }
}
