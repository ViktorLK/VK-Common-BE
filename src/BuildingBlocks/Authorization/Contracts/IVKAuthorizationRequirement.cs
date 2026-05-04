using Microsoft.AspNetCore.Authorization;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Defines a VK-specific contract for authorization requirements that include a default error.
/// This aligns with the VKResult pattern by ensuring every requirement knows its failure context.
/// </summary>
public interface IVKAuthorizationRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Gets the default error associated with the requirement failure.
    /// </summary>
    VKError DefaultError { get; }
}
