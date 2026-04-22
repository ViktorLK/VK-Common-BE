using System.Collections.Generic;

namespace VK.Blocks.Core;

/// <summary>
/// Defines a provider that exposes authentication schemes to be included in semantic group policies.
/// This interface allows the Authentication block to publish its schemes for consumption by the Authorization block
/// without direct dependency.
/// </summary>
public interface IVKSemanticSchemeProvider
{
    /// <summary>
    /// Gets schemes that should be included in the 'User' group policy.
    /// </summary>
    IEnumerable<string> GetUserSchemes();

    /// <summary>
    /// Gets schemes that should be included in the 'Service' group policy.
    /// </summary>
    IEnumerable<string> GetServiceSchemes();

    /// <summary>
    /// Gets schemes that should be included in the 'Internal' group policy.
    /// </summary>
    IEnumerable<string> GetInternalSchemes();
}
