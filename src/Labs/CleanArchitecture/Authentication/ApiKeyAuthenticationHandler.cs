using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using VK.Lab.CleanArchitecture.Constants;

namespace VK.Lab.CleanArchitecture.Authentication
{
    /// <summary>
    /// APIキー認証ハンドラー
    /// </summary>
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private const string ApiKeyHeaderName = AuthenticationConstants.ApiKeyHeaderName;
        private readonly IConfiguration _configuration;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IConfiguration configuration)
            : base(options, logger, encoder)
        {
            _configuration = configuration;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // リクエストヘッダーにAPIキーが含まれているか確認
            if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValues))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var providedApiKey = apiKeyHeaderValues.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(providedApiKey))
            {
                return Task.FromResult(AuthenticateResult.Fail(MessageConstants.Errors.InvalidApiKey));
            }

            // APIキーを検証
            var validApiKeys = _configuration.GetSection(ConfigurationKeys.Authentication.ApiKeys).Get<List<ApiKeyConfig>>();

            if (validApiKeys == null || !validApiKeys.Any())
            {
                return Task.FromResult(AuthenticateResult.Fail(MessageConstants.Errors.NoValidApiKeys));
            }

            var apiKeyConfig = validApiKeys.FirstOrDefault(k => k.Key == providedApiKey);

            if (apiKeyConfig == null)
            {
                return Task.FromResult(AuthenticateResult.Fail(MessageConstants.Errors.InvalidApiKey));
            }

            // Claimsを作成
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, apiKeyConfig.Owner),
                new Claim(ClaimTypes.NameIdentifier, apiKeyConfig.Owner),
                new Claim(AuthenticationConstants.Claims.ApiKeyId, apiKeyConfig.Id.ToString())
            };

            // ロールを追加
            foreach (var role in apiKeyConfig.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    /// <summary>
    /// APIキー設定モデル
    /// </summary>
    public class ApiKeyConfig
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }
}
