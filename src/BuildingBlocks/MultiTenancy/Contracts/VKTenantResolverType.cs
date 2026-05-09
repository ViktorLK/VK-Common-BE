namespace VK.Blocks.MultiTenancy;

/// <summary>
/// Defines the available tenant resolver strategies.
/// </summary>
public enum VKTenantResolverType
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
