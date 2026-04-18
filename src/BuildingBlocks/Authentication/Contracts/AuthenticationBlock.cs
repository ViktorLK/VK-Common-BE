using VK.Blocks.Core.DependencyInjection;

namespace VK.Blocks.Authentication.Contracts;

/// <summary>
/// A marker type for the VK.Blocks.Authentication building block.
/// </summary>
public sealed class AuthenticationBlock : IVKBlock
{
    /// <summary>
    /// The unique identifier for the Authentication building block.
    /// </summary>
    public const string Identifier = "Authentication";

    /// <inheritdoc />
    public static string BlockName => Identifier;
}


