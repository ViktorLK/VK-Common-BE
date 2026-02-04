using VK_Common_BE.Constants;

namespace VK_Common_BE.Extensions
{
    /// <summary>
    /// ミドルウェア設定拡張メソッド
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Swaggerミドルウェアを設定
        /// </summary>
        public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(RouteConstants.Swagger.JsonEndpoint, $"{RouteConstants.Swagger.Title} {RouteConstants.Swagger.Version}");
                c.RoutePrefix = RouteConstants.Swagger.RoutePrefix;
            });

            return app;
        }

        /// <summary>
        /// アプリケーションミドルウェアパイプラインを設定
        /// </summary>
        public static IApplicationBuilder ConfigureMiddleware(
            this WebApplication app)
        {
            // 開発環境設定
            if (app.Environment.IsDevelopment())
            {
                app.UseSwaggerDocumentation();
            }

            // 基本ミドルウェア
            // app.UseHttpsRedirection();  // HTTP のみ使用する場合はコメントアウト
            app.UseCors();

            // 認証と認可（順序が重要）
            app.UseAuthentication();  // UseAuthorizationの前に必要
            app.UseAuthorization();

            // ルーティング
            app.MapHealthChecks("/health");
            app.MapGraphQL();
            app.MapControllers();

            return app;
        }
    }
}
