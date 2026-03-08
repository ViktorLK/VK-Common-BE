using Microsoft.AspNetCore.Authorization;

namespace VK.Blocks.Authorization.Features.Permissions;

public sealed record PermissionRequirement(string Permission) : Microsoft.AspNetCore.Authorization.IAuthorizationRequirement;


