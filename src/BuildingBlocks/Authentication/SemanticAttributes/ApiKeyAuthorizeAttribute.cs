using Microsoft.AspNetCore.Authorization;

namespace VK.Blocks.Authentication;

/// <summary>
/// Specialized authorization attribute for ApiKey-based authentication.
/// Automatically applies the <see cref="VKAuthenticationConstants.ApiKeyPolicy"/>.
/// </summary>
public sealed class ApiKeyAuthorizeAttribute() : AuthorizeAttribute(VKAuthenticationConstants.ApiKeyPolicy);
