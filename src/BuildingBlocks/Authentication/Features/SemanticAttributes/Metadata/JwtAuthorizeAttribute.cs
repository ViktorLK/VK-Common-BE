using Microsoft.AspNetCore.Authorization;
using VK.Blocks.Authentication.Common;

namespace VK.Blocks.Authentication.Features.SemanticAttributes.Metadata;

/// <summary>
/// Specialized authorization attribute for JWT-based authentication.
/// Automatically applies the <see cref="AuthenticationConstants.JwtPolicy"/>.
/// </summary>
public sealed class JwtAuthorizeAttribute() : AuthorizeAttribute(AuthenticationConstants.JwtPolicy);
