using VK.Blocks.Core.DependencyInjection;

namespace VK.Blocks.Authorization.Contracts;

/// <summary>
/// A marker type for the VK.Blocks.Authorization building block.
/// </summary>
public sealed class AuthorizationBlock : IVKBlock
{
    /// <summary>
    /// The unique identifier for the Authorization building block.
    /// </summary>
    public const string Identifier = "Authorization";

    /// <inheritdoc />
    public static string BlockName => Identifier;
}


