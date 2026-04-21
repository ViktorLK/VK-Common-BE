using Microsoft.AspNetCore.Authorization;

namespace VK.Blocks.Authentication;

/// <summary>
/// Specialized authorization attribute for logical authentication groups.
/// Maps the provided group name to a predefined policy (e.g., "VK.Group.User").
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AuthGroupAttribute"/> class.
/// </remarks>
/// <param name="groupName">The full policy name of the authentication group (e.g., VKAuthPolicies.GroupUser).</param>
public sealed class AuthGroupAttribute(string groupName) : AuthorizeAttribute(groupName);
