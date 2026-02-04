using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace VK_Common_BE.Authentication
{
    /// <summary>
    /// 認証スキームセレクター - リクエストヘッダーに基づいて認証方法を自動選択
    /// </summary>
    public class DynamicAuthenticationSchemeSelector : IAuthenticationSchemeProvider
    {
        private readonly AuthenticationSchemeProvider _defaultProvider;
        private const string ApiKeyHeaderName = "X-Api-Key";

        public DynamicAuthenticationSchemeSelector(IOptions<AuthenticationOptions> options)
        {
            _defaultProvider = new AuthenticationSchemeProvider(options);
        }

        public Task<AuthenticationScheme?> GetDefaultAuthenticateSchemeAsync()
        {
            return _defaultProvider.GetDefaultAuthenticateSchemeAsync();
        }

        public Task<AuthenticationScheme?> GetDefaultChallengeSchemeAsync()
        {
            return _defaultProvider.GetDefaultChallengeSchemeAsync();
        }

        public Task<AuthenticationScheme?> GetDefaultForbidSchemeAsync()
        {
            return _defaultProvider.GetDefaultForbidSchemeAsync();
        }

        public Task<AuthenticationScheme?> GetDefaultSignInSchemeAsync()
        {
            return _defaultProvider.GetDefaultSignInSchemeAsync();
        }

        public Task<AuthenticationScheme?> GetDefaultSignOutSchemeAsync()
        {
            return _defaultProvider.GetDefaultSignOutSchemeAsync();
        }

        public Task<IEnumerable<AuthenticationScheme>> GetRequestHandlerSchemesAsync()
        {
            return _defaultProvider.GetRequestHandlerSchemesAsync();
        }

        public Task<AuthenticationScheme?> GetSchemeAsync(string name)
        {
            return _defaultProvider.GetSchemeAsync(name);
        }

        public Task<IEnumerable<AuthenticationScheme>> GetAllSchemesAsync()
        {
            return _defaultProvider.GetAllSchemesAsync();
        }

        public void AddScheme(AuthenticationScheme scheme)
        {
            _defaultProvider.AddScheme(scheme);
        }

        public void RemoveScheme(string name)
        {
            _defaultProvider.RemoveScheme(name);
        }

        public bool TryAddScheme(AuthenticationScheme scheme)
        {
            return _defaultProvider.TryAddScheme(scheme);
        }
    }
}
