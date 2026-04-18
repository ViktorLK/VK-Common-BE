using VK.Blocks.Core.DependencyInjection;

namespace VK.Blocks.Authentication.OpenIdConnect.Contracts;

/// <summary>
/// A marker type for the VK.Blocks.Authentication.OpenIdConnect building block.
/// </summary>
public sealed class OidcBlock : IVKBlock
{
    /// <summary>
    /// The unique identifier for the OIDC building block.
    /// </summary>
    public const string Identifier = "Authentication.Oidc";

    /// <inheritdoc />
    public static string BlockName => Identifier;
}


