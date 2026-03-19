namespace VK.Blocks.MultiTenancy.Options;

/// <summary>
/// Defines the available tenant resolver strategies.
/// </summary>
public enum TenantResolverType
{
    /// <summary>Resolves from HTTP header.</summary>
    Header,

    /// <summary>Resolves from JWT claims.</summary>
    Claims,

    /// <summary>Resolves from request domain name.</summary>
    Domain,

    /// <summary>Resolves from query string (development only).</summary>
    QueryString
}
