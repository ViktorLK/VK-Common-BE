using Microsoft.AspNetCore.Authorization;

namespace VK.Blocks.Authentication;

/// <summary>
/// Specialized authorization attribute for JWT-based authentication.
/// Automatically applies the <see cref="VKAuthenticationConstants.JwtPolicy"/>.
/// </summary>
public sealed class JwtAuthorizeAttribute() : AuthorizeAttribute(VKAuthenticationConstants.JwtPolicy);
