using Microsoft.AspNetCore.Authorization;
using VK.Blocks.Authentication.Common;

namespace VK.Blocks.Authentication.Features.SemanticAttributes;

/// <summary>
/// Specialized authorization attribute for logical authentication groups.
/// Maps the provided group name to a predefined policy (e.g., "VK.Group.User").
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AuthGroupAttribute"/> class.
/// </remarks>
/// <param name="groupName">The name of the authentication group (e.g., AuthGroups.User).</param>
public sealed class AuthGroupAttribute(string groupName) : AuthorizeAttribute($"{AuthenticationConstants.GroupPolicyPrefix}{groupName}");
