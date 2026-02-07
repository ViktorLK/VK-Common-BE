namespace VK.Lab.LayeredArchitecture.Constants
{
    /// <summary>
    /// メチE��ージ定数
    /// </summary>
    public static class MessageConstants
    {
        /// <summary>
        /// エラーメチE��ージ
        /// </summary>
        public static class Errors
        {
            /// <summary>
            /// 製品が見つからなぁE��合�EメチE��ージチE��プレーチE
            /// </summary>
            public const string ProductNotFound = "Product with ID {0} not found";

            /// <summary>
            /// 製品IDが一致しなぁE��合�EメチE��ージ
            /// </summary>
            public const string ProductIdMismatch = "Product ID mismatch";

            /// <summary>
            /// 無効なAPIキーのメチE��ージ
            /// </summary>
            public const string InvalidApiKey = "Invalid API Key";

            /// <summary>
            /// 有効なAPIキーが設定されてぁE��ぁE��合�EメチE��ージ
            /// </summary>
            public const string NoValidApiKeys = "No valid API Keys configured";

            /// <summary>
            /// IPが�Eワイトリストに含まれてぁE��ぁE��合�EメチE��ージ
            /// </summary>
            public const string IpNotWhitelisted = "IP address not whitelisted";

            /// <summary>
            /// APIキーは読み取り専用のメチE��ージ
            /// </summary>
            public const string ApiKeyReadOnlyAccess = "API Key authentication only allows GET requests. Please use Azure B2C authentication for write operations.";
        }

        /// <summary>
        /// ログメチE��ージ
        /// </summary>
        public static class Logging
        {
            /// <summary>
            /// Azure B2C認証失敗メチE��ージチE��プレーチE
            /// </summary>
            public const string AzureB2CAuthFailed = "Azure B2C authentication failed: {0}";

            /// <summary>
            /// Azure B2Cト�Eクン検証成功メチE��ージ
            /// </summary>
            public const string AzureB2CTokenValidated = "Azure B2C token validated successfully";
        }
    }
}
