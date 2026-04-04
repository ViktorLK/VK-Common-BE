using Microsoft.AspNetCore.Authorization;
using VK.Blocks.Authentication.Common;

namespace VK.Blocks.Authentication.Features.SemanticAttributes;

/// <summary>
/// Specialized authorization attribute for ApiKey-based authentication.
/// Automatically applies the <see cref="AuthenticationConstants.ApiKeyPolicy"/>.
/// </summary>
public sealed class ApiKeyAuthorizeAttribute() : AuthorizeAttribute(AuthenticationConstants.ApiKeyPolicy);
