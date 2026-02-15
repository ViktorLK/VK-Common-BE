using Microsoft.AspNetCore.Authorization;
using VK.Lab.CleanArchitecture.Constants;

namespace VK.Lab.CleanArchitecture.Authorization
{
    /// <summary>
    /// APIキー認証の読み取り専用チェックハンドラー
    /// APIキーで認証された場合、GETリクエストのみ許可
    /// </summary>
    public class ApiKeyReadOnlyHandler : AuthorizationHandler<ApiKeyReadOnlyRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApiKeyReadOnlyHandler> _logger;

        public ApiKeyReadOnlyHandler(
            IHttpContextAccessor httpContextAccessor,
            ILogger<ApiKeyReadOnlyHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ApiKeyReadOnlyRequirement requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            // ユーザーが認証されているか確認
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            // APIキー認証スキームを使用しているか確認
            var authenticationType = context.User.Identity?.AuthenticationType;
            var isApiKeyAuth = !string.IsNullOrEmpty(authenticationType) &&
                authenticationType.Equals(
                    AuthenticationConstants.Schemes.ApiKey,
                    StringComparison.OrdinalIgnoreCase);

            // APIキー認証の場合、GETメソッドのみ許可
            if (isApiKeyAuth)
            {
                var httpMethod = httpContext.Request.Method;

                if (!httpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning(
                        "API Key authentication attempted to use {Method} method. Only GET is allowed.",
                        httpMethod);
                    context.Fail();
                    return Task.CompletedTask;
                }

                _logger.LogInformation("API Key authentication - GET request authorized");
            }

            // Azure B2C認証またはGETリクエストの場合は成功
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
