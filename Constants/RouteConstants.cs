namespace VK_Common_BE.Constants
{
    /// <summary>
    /// APIルート定数
    /// </summary>
    public static class RouteConstants
    {
        /// <summary>
        /// APIベースルートプレフィックス
        /// </summary>
        public const string ApiBase = "api";

        /// <summary>
        /// コントローラールート
        /// </summary>
        public static class Controllers
        {
            /// <summary>
            /// 製品コントローラールート: api/products
            /// </summary>
            public const string Products = $"{ApiBase}/[controller]";
        }

        /// <summary>
        /// Swaggerルート
        /// </summary>
        public static class Swagger
        {
            /// <summary>
            /// Swagger UIルートプレフィックス
            /// </summary>
            public const string RoutePrefix = "swagger";

            /// <summary>
            /// Swagger JSONエンドポイント
            /// </summary>
            public const string JsonEndpoint = "/swagger/v1/swagger.json";

            /// <summary>
            /// APIバージョン
            /// </summary>
            public const string Version = "v1";

            /// <summary>
            /// APIタイトル
            /// </summary>
            public const string Title = "VK Common BE API";

            /// <summary>
            /// API説明
            /// </summary>
            public const string Description = "VK Common Backend WebAPI with EntityFramework Core";
        }
    }
}
