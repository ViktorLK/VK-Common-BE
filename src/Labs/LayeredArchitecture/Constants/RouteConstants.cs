namespace VK.Lab.LayeredArchitecture.Constants
{
    /// <summary>
    /// APIルート定数
    /// </summary>
    public static class RouteConstants
    {
        /// <summary>
        /// APIベ�Eスルート�EレフィチE��ス
        /// </summary>
        public const string ApiBase = "api";

        /// <summary>
        /// コントローラールーチE
        /// </summary>
        public static class Controllers
        {
            /// <summary>
            /// 製品コントローラールーチE api/products
            /// </summary>
            public const string Products = $"{ApiBase}/[controller]";
        }

        /// <summary>
        /// SwaggerルーチE
        /// </summary>
        public static class Swagger
        {
            /// <summary>
            /// Swagger UIルート�EレフィチE��ス
            /// </summary>
            public const string RoutePrefix = "swagger";

            /// <summary>
            /// Swagger JSONエンド�EインチE
            /// </summary>
            public const string JsonEndpoint = "/swagger/v1/swagger.json";

            /// <summary>
            /// APIバ�Eジョン
            /// </summary>
            public const string Version = "v1";

            /// <summary>
            /// APIタイトル
            /// </summary>
            public const string Title = "VK Common BE API";

            /// <summary>
            /// API説昁E
            /// </summary>
            public const string Description = "VK Common Backend WebAPI with EntityFramework Core";
        }
    }
}
