namespace VK.Blocks.MultiTenancy;

/// <summary>
/// Defines the types of tenant resolvers supported by the system.
/// </summary>
public enum VKTenantResolverType : byte
{
    /// <summary>
    /// Resolves tenant from the HTTP request header.
    /// </summary>
    Header = 0,

    /// <summary>
    /// Resolves tenant from the request host / sub-domain.
    /// </summary>
    Host = 1,

    /// <summary>
    /// Resolves tenant from query string (typically development/testing only).
    /// </summary>
    QueryString = 2,

    /// <summary>
    /// Resolves tenant from authenticated user claims / JWT.
    /// </summary>
    Claims = 3
}
